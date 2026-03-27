using System.Text.Json.Serialization;

namespace PosService.Application.DTOs;

/// <summary>
/// Trả về chi tiết sale (khi GET /api/sales/{id})
/// Bao gồm: Sale info + Items (không barcode) + Payments
/// </summary>
public class SaleDetailDto
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("saleNumber")]
    public string SaleNumber { get; set; } = string.Empty;

    [JsonPropertyName("storeId")]
    public Guid StoreId { get; set; }

    [JsonPropertyName("cashierId")]
    public Guid CashierId { get; set; }

    [JsonPropertyName("customerId")]
    public Guid? CustomerId { get; set; }

    [JsonPropertyName("saleDate")]
    public DateTime SaleDate { get; set; }

    [JsonPropertyName("subtotal")]
    public decimal Subtotal { get; set; }

    [JsonPropertyName("totalAmount")]
    public decimal TotalAmount { get; set; }

    [JsonPropertyName("paymentMethod")]
    public string PaymentMethod { get; set; } = string.Empty;

    [JsonPropertyName("paymentStatus")]
    public string PaymentStatus { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("notes")]
    public string? Notes { get; set; }

    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("items")]
    public List<SaleItemDetailDto> Items { get; set; } = new();

    [JsonPropertyName("payments")]
    public List<PaymentDetailDto> Payments { get; set; } = new();
}

/// <summary>
/// Chi tiết một sale item (không barcode, không discount)
/// </summary>
public class SaleItemDetailDto
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("productId")]
    public Guid ProductId { get; set; }

    [JsonPropertyName("productName")]
    public string ProductName { get; set; } = string.Empty;

    [JsonPropertyName("sku")]
    public string Sku { get; set; } = string.Empty;

    [JsonPropertyName("quantity")]
    public int Quantity { get; set; }

    [JsonPropertyName("unitPrice")]
    public decimal UnitPrice { get; set; }

    [JsonPropertyName("lineTotal")]
    public decimal LineTotal { get; set; }
}

/// <summary>
/// Chi tiết payment
/// </summary>
public class PaymentDetailDto
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("paymentMethod")]
    public string PaymentMethod { get; set; } = string.Empty;

    [JsonPropertyName("amount")]
    public decimal Amount { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("cashReceived")]
    public decimal? CashReceived { get; set; }

    [JsonPropertyName("cashChange")]
    public decimal? CashChange { get; set; }

    [JsonPropertyName("transactionReference")]
    public string? TransactionReference { get; set; }

    [JsonPropertyName("paymentDate")]
    public DateTime PaymentDate { get; set; }

    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; }
}
