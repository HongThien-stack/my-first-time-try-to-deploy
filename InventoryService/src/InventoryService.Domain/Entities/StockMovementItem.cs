namespace InventoryService.Domain.Entities;

public class StockMovementItem
{
    public Guid Id { get; set; }
    public Guid MovementId { get; set; }
    public Guid BatchId { get; set; }
    public int Quantity { get; set; }
    public DateTime CreatedAt { get; set; }

    // Navigation properties
    public StockMovement? StockMovement { get; set; }
    public ProductBatch? ProductBatch { get; set; }
}
