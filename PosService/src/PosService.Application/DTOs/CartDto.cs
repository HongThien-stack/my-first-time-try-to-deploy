namespace PosService.Application.DTOs;

public class CartDto
{
    public Guid Id { get; set; }
    public string SaleNumber { get; set; } = string.Empty;
    public Guid StoreId { get; set; }
    public Guid CashierId { get; set; }
    public Guid? CustomerId { get; set; }
    public decimal Subtotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = string.Empty; // PENDING, COMPLETED, CANCELLED
    public string PaymentStatus { get; set; } = string.Empty; // PENDING, PAID, FAILED
    public string PaymentMethod { get; set; } = string.Empty; // PENDING, CASH, CARD, VNPAY, MOMO
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public List<CartItemDto> Items { get; set; } = new List<CartItemDto>();
}

public class CartItemDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? ProductSku { get; set; }
    public string? Barcode { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineDiscount { get; set; }
    public decimal LineTotal { get; set; }
}
