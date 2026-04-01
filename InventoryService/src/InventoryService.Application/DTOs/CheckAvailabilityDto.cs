using System.Text.Json.Serialization;

namespace InventoryService.Application.DTOs;

/// <summary>
/// Request để kiểm tra sản phẩm có đủ tồn kho
/// </summary>
public class CheckAvailabilityRequestDto
{
    [JsonPropertyName("storeId")]
    public Guid StoreId { get; set; }

    [JsonPropertyName("items")]
    public List<CheckAvailabilityItemDto> Items { get; set; } = new();
}

/// <summary>
/// Chi tiết từng sản phẩm cần kiểm tra
/// </summary>
public class CheckAvailabilityItemDto
{
    [JsonPropertyName("productId")]
    public Guid ProductId { get; set; }

    [JsonPropertyName("quantity")]
    public int Quantity { get; set; }
}

/// <summary>
/// Response từ check availability
/// </summary>
public class CheckAvailabilityResponseDto
{
    [JsonPropertyName("isAvailable")]
    public bool IsAvailable { get; set; }

    [JsonPropertyName("unavailableItems")]
    public List<UnavailableItemDto> UnavailableItems { get; set; } = new();
}

/// <summary>
/// Chi tiết sản phẩm không đủ tồn kho
/// </summary>
public class UnavailableItemDto
{
    [JsonPropertyName("productId")]
    public Guid ProductId { get; set; }

    [JsonPropertyName("productName")]
    public string ProductName { get; set; } = string.Empty;

    [JsonPropertyName("requestedQty")]
    public int RequestedQty { get; set; }

    [JsonPropertyName("availableQty")]
    public int AvailableQty { get; set; }
}
