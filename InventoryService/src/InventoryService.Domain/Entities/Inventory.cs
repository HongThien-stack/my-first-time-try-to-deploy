namespace InventoryService.Domain.Entities;

public class Inventory
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; } // ProductDB.products.id
    public string LocationType { get; set; } = "WAREHOUSE"; // WAREHOUSE | STORE
    public Guid LocationId { get; set; } // warehouse_id or store_id
    public int Quantity { get; set; } = 0;
    public int ReservedQuantity { get; set; } = 0;
    public int AvailableQuantity => Quantity - ReservedQuantity; // Computed: matches DB persisted column
    public int? MinStockLevel { get; set; } = 10;
    public int? MaxStockLevel { get; set; } = 1000;
    public DateTime? LastStockCheck { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
