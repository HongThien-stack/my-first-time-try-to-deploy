namespace InventoryService.Domain.Entities;

public class StockMovement
{
    public Guid Id { get; set; }
    public Guid WarehouseId { get; set; }
    public Guid? StoreId { get; set; } // Reference to OrderDB.stores.id (for EXPORT)
    public string MovementType { get; set; } = string.Empty; // IMPORT | EXPORT
    public string? SourceInfo { get; set; }
    public string? Note { get; set; }
    public Guid CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }

    // Navigation properties
    public Warehouse? Warehouse { get; set; }
    public ICollection<StockMovementItem> StockMovementItems { get; set; } = new List<StockMovementItem>();
}
