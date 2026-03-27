using System.Text.Json.Serialization;

namespace InventoryService.Application.DTOs;

/// <summary>
/// Request để trừ tồn kho (Internal endpoint - dùng bởi PosService)
/// </summary>
public class ReduceInventoryRequestDto
{
    [JsonPropertyName("storeId")]
    public Guid StoreId { get; set; }

    [JsonPropertyName("items")]
    public List<ReduceInventoryItemDto> Items { get; set; } = new();
}

/// <summary>
/// Chi tiết từng sản phẩm cần trừ
/// </summary>
public class ReduceInventoryItemDto
{
    [JsonPropertyName("productId")]
    public Guid ProductId { get; set; }

    [JsonPropertyName("quantity")]
    public int Quantity { get; set; }
}

/// <summary>
/// Response từ reduce inventory
/// </summary>
public class ReduceInventoryResponseDto
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    [JsonPropertyName("reducedItems")]
    public List<ReducedInventoryItemDto> ReducedItems { get; set; } = new();
}

/// <summary>
/// Chi tiết sản phẩm đã trừ được
/// </summary>
public class ReducedInventoryItemDto
{
    [JsonPropertyName("productId")]
    public Guid ProductId { get; set; }

    [JsonPropertyName("quantityReduced")]
    public int QuantityReduced { get; set; }

    [JsonPropertyName("remainingQuantity")]
    public int RemainingQuantity { get; set; }
}
