using Microsoft.Extensions.Logging;
using PosService.Application.DTOs;
using PosService.Application.Interfaces;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Globalization;

namespace PosService.Application.Services;

public class PdfReceiptService : IPdfReceiptService
{
    private readonly ILogger<PdfReceiptService> _logger;

    public PdfReceiptService(ILogger<PdfReceiptService> logger)
    {
        _logger = logger;
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public Task<byte[]> GenerateReceiptPdfAsync(ReceiptResponseDto receipt, CancellationToken cancellationToken = default)
    {
        var store = ResolveStoreInfo(receipt.StoreId);
        var cashierName = ResolveCashierName(receipt.CashierId);
        var totalQuantity = receipt.Items.Sum(x => x.Quantity);

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(24);
                page.DefaultTextStyle(x => x.FontSize(9.5f));

                page.Header().Column(column =>
                {
                    column.Spacing(3);
                    column.Item()
                        .Background(Colors.Green.Darken3)
                        .PaddingVertical(8)
                        .PaddingHorizontal(10)
                        .AlignCenter()
                        .Text("BACH HOA XANH")
                        .Bold()
                        .FontSize(18)
                        .FontColor(Colors.White);

                    column.Item().AlignCenter().Text("HOA DON BAN LE / RECEIPT").Bold().FontSize(11);
                    column.Item().AlignCenter().Text("MST: 0310471746").FontSize(8.5f).FontColor(Colors.Grey.Darken2);
                    column.Item().AlignCenter().Text(store.Address).FontSize(8.5f).FontColor(Colors.Grey.Darken2);
                    column.Item().AlignCenter().Text("Hotline: 1900 1908").FontSize(8.5f).FontColor(Colors.Grey.Darken2);
                    column.Item().PaddingTop(4).LineHorizontal(1).LineColor(Colors.Grey.Lighten1);
                });

                page.Content().Column(column =>
                {
                    column.Spacing(8);

                    column.Item().Text("THONG TIN GIAO DICH").Bold().FontSize(10.5f).FontColor(Colors.Green.Darken3);

                    column.Item().Table(table =>
                    {
                        table.ColumnsDefinition(cols =>
                        {
                            cols.RelativeColumn(1.9f);
                            cols.RelativeColumn(3.1f);
                        });

                        AddMetaRow(table, "So HD", receipt.SaleNumber);
                        AddMetaRow(table, "Ngay gio", receipt.SaleDate.ToString("dd/MM/yyyy HH:mm:ss"));
                        AddMetaRow(table, "Cua hang", store.Name);
                        AddMetaRow(table, "Quay", store.CounterCode);
                        AddMetaRow(table, "Thu ngan", cashierName);
                        AddMetaRow(table, "Thanh toan", receipt.PaymentMethod);
                    });

                    column.Item().PaddingTop(2).Text("CHI TIET SAN PHAM").Bold().FontSize(10.5f).FontColor(Colors.Green.Darken3);

                    column.Item().Table(table =>
                    {
                        table.ColumnsDefinition(cols =>
                        {
                            cols.RelativeColumn(0.7f);
                            cols.RelativeColumn(3.2f);
                            cols.RelativeColumn(0.9f);
                            cols.RelativeColumn(1.8f);
                            cols.RelativeColumn(1.5f);
                            cols.RelativeColumn(2f);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Element(HeaderCellStyle).AlignCenter().Text("STT").Bold();
                            header.Cell().Element(HeaderCellStyle).Text("Mat hang").Bold();
                            header.Cell().Element(HeaderCellStyle).AlignRight().Text("SL").Bold();
                            header.Cell().Element(HeaderCellStyle).AlignRight().Text("Don gia").Bold();
                            header.Cell().Element(HeaderCellStyle).AlignRight().Text("Giam").Bold();
                            header.Cell().Element(HeaderCellStyle).AlignRight().Text("Thanh tien").Bold();
                        });

                        if (receipt.Items.Count == 0)
                        {
                            table.Cell().Element(CellStyle).AlignCenter().Text("-");
                            table.Cell().Element(CellStyle).Text("Khong co du lieu san pham").FontColor(Colors.Grey.Darken1);
                            table.Cell().Element(CellStyle).AlignRight().Text("-");
                            table.Cell().Element(CellStyle).AlignRight().Text("-");
                            table.Cell().Element(CellStyle).AlignRight().Text("-");
                            table.Cell().Element(CellStyle).AlignRight().Text("-");
                        }

                        for (var i = 0; i < receipt.Items.Count; i++)
                        {
                            var item = receipt.Items[i];
                            table.Cell().Element(CellStyle).AlignCenter().Text((i + 1).ToString());
                            table.Cell().Element(CellStyle).Text($"{item.ProductName} ({item.Sku})");
                            table.Cell().Element(CellStyle).AlignRight().Text(item.Quantity.ToString());
                            table.Cell().Element(CellStyle).AlignRight().Text(ToVndCompact(item.UnitPrice));
                            table.Cell().Element(CellStyle).AlignRight().Text(ToVndCompact(item.Discount));
                            table.Cell().Element(CellStyle).AlignRight().Text(ToVndCompact(item.LineTotal));
                        }

                        static IContainer HeaderCellStyle(IContainer container)
                        {
                            return container
                                .Background(Colors.Yellow.Lighten3)
                                .BorderBottom(1)
                                .BorderColor(Colors.Grey.Lighten1)
                                .PaddingVertical(5)
                                .PaddingHorizontal(4);
                        }

                        static IContainer CellStyle(IContainer container)
                        {
                            return container.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(4);
                        }
                    });

                    column.Item().PaddingTop(4).AlignRight().Width(240).Column(total =>
                    {
                        total.Spacing(2);
                        total.Item().Text($"Tong SL: {totalQuantity}");
                        total.Item().Text($"Tam tinh: {ToVnd(receipt.Subtotal)}");
                        total.Item().Text($"Giam gia: {ToVnd(receipt.Discount)}");
                        total.Item().Text($"Thue (VAT): {ToVnd(receipt.Tax)}");
                        total.Item().PaddingTop(2).LineHorizontal(1).LineColor(Colors.Grey.Lighten1);
                        total.Item()
                            .Background(Colors.Green.Darken3)
                            .PaddingVertical(4)
                            .PaddingHorizontal(6)
                            .AlignRight()
                            .Text($"Tong thanh toan: {ToVnd(receipt.Total)}")
                            .Bold()
                            .FontColor(Colors.White)
                            .FontSize(10.5f);
                    });

                    column.Item().PaddingTop(3).Text("CHI TIET THANH TOAN").Bold().FontSize(10.5f).FontColor(Colors.Green.Darken3);
                    column.Item().Text($"Hinh thuc: {receipt.PaymentMethod}");
                    column.Item().Text($"Trang thai: {receipt.PaymentStatus ?? "N/A"}");

                    if (receipt.CashReceived.HasValue)
                    {
                        column.Item().Text($"Tien khach dua: {ToVnd(receipt.CashReceived.Value)}");
                    }

                    if (receipt.CashChange.HasValue)
                    {
                        column.Item().Text($"Tien thua: {ToVnd(receipt.CashChange.Value)}");
                    }

                    if (!string.IsNullOrWhiteSpace(receipt.TransactionReference))
                    {
                        column.Item().Text($"Ma giao dich: {receipt.TransactionReference}");
                    }
                });

                page.Footer().Column(column =>
                {
                    column.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten1);
                    column.Item().PaddingTop(4).AlignCenter().Text("Cam on Quy khach va hen gap lai!").Bold().FontSize(9);
                    column.Item().AlignCenter().Text("Hoa don duoc tao boi he thong POS").FontSize(8).FontColor(Colors.Grey.Darken1);
                    column.Item().AlignCenter().Text($"In luc: {DateTime.Now:dd/MM/yyyy HH:mm:ss}").FontSize(8).FontColor(Colors.Grey.Darken1);
                });
            });
        });

        _logger.LogInformation("Generated receipt PDF for sale {SaleNumber}", receipt.SaleNumber);
        return Task.FromResult(document.GeneratePdf());
    }

    private static void AddMetaRow(QuestPDF.Fluent.TableDescriptor table, string label, string value)
    {
        table.Cell().PaddingVertical(2).Text(label).SemiBold();
        table.Cell().PaddingVertical(2).Text(value);
    }

    private static string ToVnd(decimal amount)
    {
        return string.Format(CultureInfo.GetCultureInfo("vi-VN"), "{0:N0} VND", amount);
    }

    private static string ToVndCompact(decimal amount)
    {
        return string.Format(CultureInfo.GetCultureInfo("vi-VN"), "{0:N0}", amount);
    }

    private static StoreInfo ResolveStoreInfo(Guid storeId)
    {
        return storeId.ToString().ToUpperInvariant() switch
        {
            "B0000001-0001-0001-0001-000000000001" => new StoreInfo(
                "Bach Hoa Xanh Thu Duc",
                "123 Le Van Viet, Thu Duc, TP.HCM",
                "Q01"),
            "B0000001-0001-0001-0001-000000000002" => new StoreInfo(
                "Bach Hoa Xanh Quan 1",
                "456 Nguyen Hue, Quan 1, TP.HCM",
                "Q02"),
            _ => new StoreInfo(
                $"Bach Hoa Xanh {storeId}",
                "Store address not configured",
                "Q00")
        };
    }

    private static string ResolveCashierName(Guid cashierId)
    {
        return cashierId.ToString().ToUpperInvariant() switch
        {
            "33333333-3333-3333-3333-333333333331" => "Cashier 01",
            "33333333-3333-3333-3333-333333333332" => "Cashier 02",
            "33333333-3333-3333-3333-333333333333" => "Cashier 03",
            _ => $"Cashier ID: {cashierId}"
        };
    }

    private sealed record StoreInfo(string Name, string Address, string CounterCode);
}
