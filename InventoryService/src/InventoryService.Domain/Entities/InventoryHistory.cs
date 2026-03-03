namespace InventoryService.Domain.Entities;

public class InventoryHistory
{
    public long Id { get; set; } // BIGINT IDENTITY
    public Guid InventoryId { get; set; }
    public Guid ProductId { get; set; }
    public string LocationType { get; set; } = string.Empty;
    public Guid LocationId { get; set; }
    public DateTime SnapshotDate { get; set; }
    public int Quantity { get; set; }
    public int ReservedQuantity { get; set; }
    public int AvailableQuantity { get; set; }
    public DateTime CreatedAt { get; set; }
}
