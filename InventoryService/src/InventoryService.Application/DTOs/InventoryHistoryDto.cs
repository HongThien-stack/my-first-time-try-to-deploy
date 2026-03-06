namespace InventoryService.Application.DTOs;

public class InventoryHistoryDto
{
    public long Id { get; set; }
    public Guid InventoryId { get; set; }
    public Guid ProductId { get; set; }
    public string LocationType { get; set; } = string.Empty;
    public Guid LocationId { get; set; }
    public DateOnly SnapshotDate { get; set; }
    public int Quantity { get; set; }
    public int ReservedQuantity { get; set; }
    public int AvailableQuantity { get; set; }
    public DateTime CreatedAt { get; set; }
}
