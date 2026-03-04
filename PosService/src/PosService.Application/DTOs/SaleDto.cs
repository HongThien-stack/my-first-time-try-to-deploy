namespace PosService.Application.DTOs;

public class SaleDto
{
    public Guid Id { get; set; }
    public string SaleNumber { get; set; } = string.Empty;
    public Guid StoreId { get; set; }
    public Guid CashierId { get; set; }
    public Guid? CustomerId { get; set; }
    public DateTime SaleDate { get; set; }
    public decimal Subtotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string PaymentStatus { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public Guid? PromotionId { get; set; }
    public string? VoucherCode { get; set; }
    public int PointsUsed { get; set; }
    public int PointsEarned { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public int ItemCount { get; set; }
}
