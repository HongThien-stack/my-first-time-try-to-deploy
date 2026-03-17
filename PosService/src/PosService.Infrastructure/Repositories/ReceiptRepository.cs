using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using PosService.Application.DTOs;
using PosService.Application.Interfaces;
using PosService.Infrastructure.Data;

namespace PosService.Infrastructure.Repositories;

public class ReceiptRepository : IReceiptRepository
{
    private readonly PosDbContext _posDbContext;
    private readonly ILogger<ReceiptRepository> _logger;

    public ReceiptRepository(PosDbContext posDbContext, ILogger<ReceiptRepository> logger)
    {
        _posDbContext = posDbContext;
        _logger = logger;
    }

    public async Task<ReceiptResponseDto?> GetReceiptBySaleIdAsync(Guid saleId, CancellationToken cancellationToken = default)
    {
        // Query header and latest payment in one round-trip.
        var sale = await _posDbContext.Sales
            .AsNoTracking()
            .Where(x => x.Id == saleId)
            .Select(x => new
            {
                x.Id,
                x.SaleNumber,
                x.SaleDate,
                x.StoreId,
                x.CashierId,
                x.Subtotal,
                x.DiscountAmount,
                x.TaxAmount,
                x.TotalAmount,
                x.PaymentMethod,
                x.PaymentStatus,
                Payment = _posDbContext.Payments
                    .AsNoTracking()
                    .Where(p => p.SaleId == x.Id)
                    .OrderByDescending(p => p.PaymentDate)
                    .Select(p => new
                    {
                        p.PaymentMethod,
                        p.Status,
                        p.CashReceived,
                        p.CashChange,
                        p.TransactionReference
                    })
                    .FirstOrDefault()
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (sale == null)
        {
            _logger.LogWarning("Sale not found for receipt. SaleId: {SaleId}", saleId);
            return null;
        }

        List<ReceiptItemDto> items;
        try
        {
            items = await _posDbContext.SaleItems
                .AsNoTracking()
                .Where(x => x.SaleId == saleId)
                .OrderBy(x => x.ProductName)
                .Select(x => new ReceiptItemDto
                {
                    ProductName = x.ProductName,
                    Sku = x.Sku,
                    Quantity = x.Quantity,
                    UnitPrice = x.UnitPrice,
                    Discount = x.DiscountAmount,
                    LineTotal = x.LineTotal
                })
                .ToListAsync(cancellationToken);
        }
        catch (SqlException ex) when (ex.Number == 208)
        {
            _logger.LogWarning(
                ex,
                "sale_items table is missing in POSDB. Returning receipt with empty item list. SaleId: {SaleId}",
                saleId);
            items = new List<ReceiptItemDto>();
        }

        return new ReceiptResponseDto
        {
            SaleId = sale.Id,
            SaleNumber = sale.SaleNumber,
            SaleDate = sale.SaleDate,
            StoreId = sale.StoreId,
            CashierId = sale.CashierId,
            Items = items,
            Subtotal = sale.Subtotal,
            Discount = sale.DiscountAmount,
            Tax = sale.TaxAmount,
            Total = sale.TotalAmount,
            PaymentMethod = sale.Payment?.PaymentMethod ?? sale.PaymentMethod,
            PaymentStatus = sale.Payment?.Status ?? sale.PaymentStatus,
            CashReceived = sale.Payment?.CashReceived,
            CashChange = sale.Payment?.CashChange,
            TransactionReference = sale.Payment?.TransactionReference
        };
    }
}
