namespace InventoryService.Domain.Entities;

public class    StockMovementItem
{
    public Guid Id { get; set; }
    public Guid MovementId { get; set; }
    public Guid ProductId { get; set; } // ProductDB.products.id
    public int Quantity { get; set; }
    public decimal? UnitPrice { get; set; } // For valuation

    // Navigation properties
    public StockMovement StockMovement { get; set; } = null!;
}
