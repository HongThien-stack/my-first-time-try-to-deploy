namespace InventoryService.Domain.Entities;

public class InventoryCheckItem
{
    public Guid Id { get; set; }
    public Guid CheckId { get; set; }
    public Guid ProductId { get; set; } // ProductDB.products.id
    public int SystemQuantity { get; set; }
    public int ActualQuantity { get; set; }
    public int Difference => ActualQuantity - SystemQuantity; // Computed property
    public string? Note { get; set; }

    // Navigation properties
    public InventoryCheck InventoryCheck { get; set; } = null!;
}
