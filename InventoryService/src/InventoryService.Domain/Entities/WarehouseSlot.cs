namespace InventoryService.Domain.Entities;

public class WarehouseSlot
{
    public Guid Id { get; set; }
    public Guid WarehouseId { get; set; }
    public string SlotCode { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public string Status { get; set; } = "AVAILABLE"; // AVAILABLE | OCCUPIED | BLOCKED
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Guid? CreatedBy { get; set; }
    public Guid? UpdatedBy { get; set; }

    // Navigation property
    public Warehouse? Warehouse { get; set; }
}
