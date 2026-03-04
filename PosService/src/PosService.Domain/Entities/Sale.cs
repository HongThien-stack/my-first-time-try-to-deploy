namespace PosService.Domain.Entities;

public class Sale
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
    public string PaymentMethod { get; set; } = string.Empty; // CASH, CARD, VNPAY, MOMO
    public string PaymentStatus { get; set; } = string.Empty; // PENDING, COMPLETED, FAILED
    public string Status { get; set; } = string.Empty; // COMPLETED, CANCELLED, PENDING
    public Guid? PromotionId { get; set; }
    public string? VoucherCode { get; set; }
    public int PointsUsed { get; set; }
    public int PointsEarned { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Navigation
    public ICollection<SaleItem> SaleItems { get; set; } = new List<SaleItem>();
}
