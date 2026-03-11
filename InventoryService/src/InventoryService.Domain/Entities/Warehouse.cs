namespace InventoryService.Domain.Entities;

public class Warehouse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Location { get; set; }
    public int Capacity { get; set; }
    public string Status { get; set; } = "ACTIVE"; // ACTIVE | INACTIVE
    public Guid? ParentId { get; set; } // Self-reference: sub-warehouse points to kho tổng
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid? CreatedBy { get; set; }

    // Navigation properties
    public Warehouse? Parent { get; set; }
    public ICollection<Warehouse> SubWarehouses { get; set; } = new List<Warehouse>();
    public ICollection<ProductBatch> ProductBatches { get; set; } = new List<ProductBatch>();
}
