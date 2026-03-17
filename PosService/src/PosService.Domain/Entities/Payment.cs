namespace PosService.Domain.Entities;

public class Payment
{
    public Guid Id { get; set; }
    public Guid SaleId { get; set; }
    public string PaymentMethod { get; set; } = string.Empty; // CASH | CARD | VNPAY | MOMO
    public decimal Amount { get; set; }
    public string Status { get; set; } = string.Empty; // PENDING | COMPLETED | FAILED

    // For cash payments
    public decimal? CashReceived { get; set; }
    public decimal? CashChange { get; set; }

    // For online payments (VNPay/Momo)
    public string? TransactionReference { get; set; }

    public DateTime PaymentDate { get; set; }
    public DateTime CreatedAt { get; set; }

    // Navigation
    public Sale? Sale { get; set; }
}
