using System.Text.Json.Serialization;

namespace InventoryService.Application.DTOs;

public class ExpiringSoonBatchDto
{
    [JsonPropertyName("batch_id")]
    public Guid BatchId { get; set; }

    [JsonPropertyName("product_id")]
    public Guid ProductId { get; set; }

    [JsonPropertyName("product_name")]
    public string ProductName { get; set; } = string.Empty;

    [JsonPropertyName("expiry_date")]
    public DateTime ExpiryDate { get; set; }

    [JsonPropertyName("remaining_days")]
    public int RemainingDays { get; set; }

    [JsonPropertyName("quantity")]
    public decimal Quantity { get; set; }

    [JsonPropertyName("is_perishable")]
    public bool IsPerishable { get; set; }
}
