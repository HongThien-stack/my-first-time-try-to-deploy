using System.Text.Json.Serialization;

namespace InventoryService.Application.DTOs;

public class BatchDetailDto
{
    [JsonPropertyName("batch_id")]
    public Guid BatchId { get; set; }

    [JsonPropertyName("product_id")]
    public Guid ProductId { get; set; }

    [JsonPropertyName("product_name")]
    public string ProductName { get; set; } = string.Empty;

    [JsonPropertyName("manufacturing_date")]
    public DateTime ManufacturingDate { get; set; }

    [JsonPropertyName("expiry_date")]
    public DateTime ExpiryDate { get; set; }

    [JsonPropertyName("shelf_life_days")]
    public int ShelfLifeDays { get; set; }

    [JsonPropertyName("is_perishable")]
    public bool IsPerishable { get; set; }

    [JsonPropertyName("quantity")]
    public decimal Quantity { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("remaining_days")]
    public int RemainingDays { get; set; }

    [JsonPropertyName("expiry_state")]
    public string ExpiryState { get; set; } = string.Empty;
}
