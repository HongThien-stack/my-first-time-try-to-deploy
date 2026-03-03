namespace InventoryService.Application.DTOs;

public class InventoryDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string LocationType { get; set; } = string.Empty; // WAREHOUSE | STORE
    public Guid LocationId { get; set; }
    public int Quantity { get; set; }
    public int ReservedQuantity { get; set; }
    public int AvailableQuantity { get; set; }
    public int MinStockLevel { get; set; }
    public int MaxStockLevel { get; set; }
    public bool IsLowStock => AvailableQuantity <= MinStockLevel;
    public DateTime? LastStockCheck { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

