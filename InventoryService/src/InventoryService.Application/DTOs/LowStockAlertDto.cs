namespace InventoryService.Application.DTOs;

/// <summary>
/// DTO for low stock alert item with enriched product information
/// </summary>
public class LowStockAlertDto
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public string LocationType { get; set; } = string.Empty; // WAREHOUSE | STORE
    public Guid LocationId { get; set; }
    public int AvailableQuantity { get; set; }
    public int MinStockLevel { get; set; }
    public int MaxStockLevel { get; set; }
    public string StockStatus { get; set; } = string.Empty; // LOW | OUT_OF_STOCK
    public DateTime? LastStockCheck { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
