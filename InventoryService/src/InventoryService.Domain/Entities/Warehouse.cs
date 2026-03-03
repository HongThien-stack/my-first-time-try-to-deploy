namespace InventoryService.Domain.Entities;

public class Warehouse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Location { get; set; }
    public int Capacity { get; set; } // Total slots
    public string Status { get; set; } = "ACTIVE"; // ACTIVE | INACTIVE
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid? CreatedBy { get; set; }

    // Navigation properties
    public ICollection<WarehouseSlot> WarehouseSlots { get; set; } = new List<WarehouseSlot>();
    public ICollection<ProductBatch> ProductBatches { get; set; } = new List<ProductBatch>();
}
