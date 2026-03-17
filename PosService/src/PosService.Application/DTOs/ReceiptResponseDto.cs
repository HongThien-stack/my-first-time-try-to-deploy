namespace PosService.Application.DTOs;

public class ReceiptResponseDto
{
    public Guid SaleId { get; set; }
    public string SaleNumber { get; set; } = string.Empty;
    public DateTime SaleDate { get; set; }
    public Guid StoreId { get; set; }
    public Guid CashierId { get; set; }
    public List<ReceiptItemDto> Items { get; set; } = new();

    public decimal Subtotal { get; set; }
    public decimal Discount { get; set; }
    public decimal Tax { get; set; }
    public decimal Total { get; set; }

    public string PaymentMethod { get; set; } = string.Empty;
    public string? PaymentStatus { get; set; }
    public decimal? CashReceived { get; set; }
    public decimal? CashChange { get; set; }
    public string? TransactionReference { get; set; }
}
