namespace InventoryService.Domain.Entities;

public class Inventory
{
    public Guid Id { get; set; }
    public Guid StoreId { get; set; } // Reference to OrderDB.stores.id
    public Guid ProductId { get; set; } // Reference to ProductDB.products.id
    public int Quantity { get; set; }
    public int AlertThreshold { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Guid? CreatedBy { get; set; }
    public Guid? UpdatedBy { get; set; }
}
