namespace InventoryService.Domain.Entities;

public class TransferItem
{
    public Guid Id { get; set; }
    public Guid TransferId { get; set; }
    public Guid ProductId { get; set; } // ProductDB.products.id
    public int RequestedQuantity { get; set; }
    public int? ShippedQuantity { get; set; }
    public int? ReceivedQuantity { get; set; }
    public int DamagedQuantity { get; set; } = 0;
    public string? Notes { get; set; }

    // Navigation properties
    public Transfer Transfer { get; set; } = null!;
}
