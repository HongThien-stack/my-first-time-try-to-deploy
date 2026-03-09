namespace InventoryService.Domain.Entities;

public class Inventory
{
    public Guid Id { get; set; }
    public Guid StoreId { get; set; } // Store location
    public Guid ProductId { get; set; } // ProductDB.products.id
    public int Quantity { get; set; } = 0;
    public int AlertThreshold { get; set; } = 10; // Alert threshold
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Guid? CreatedBy { get; set; }
    public Guid? UpdatedBy { get; set; }
}
