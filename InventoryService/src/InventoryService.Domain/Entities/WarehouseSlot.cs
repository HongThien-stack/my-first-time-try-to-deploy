namespace InventoryService.Domain.Entities;

public class WarehouseSlot
{
    public Guid Id { get; set; }
    public Guid WarehouseId { get; set; }
    public string SlotCode { get; set; } = string.Empty; // A-01-01
    public string? Zone { get; set; } // A, B, C
    public int? RowNumber { get; set; }
    public int? ColumnNumber { get; set; }
    public string Status { get; set; } = "EMPTY"; // EMPTY | OCCUPIED | RESERVED | MAINTENANCE
    public DateTime CreatedAt { get; set; }

    // Navigation property
    public Warehouse Warehouse { get; set; } = null!;
    public ICollection<ProductBatch> ProductBatches { get; set; } = new List<ProductBatch>();
}
