namespace InventoryService.Domain.Entities;

public class InventoryLog
{
    public long Id { get; set; } // BIGINT IDENTITY
    public Guid InventoryId { get; set; }
    public Guid ProductId { get; set; }
    public string Action { get; set; } = string.Empty; // ADJUST | RECEIVE | TRANSFER | SALE | DAMAGE
    public int OldQuantity { get; set; }
    public int NewQuantity { get; set; }
    public int QuantityChange => NewQuantity - OldQuantity; // Computed property
    public string? Reason { get; set; }
    public Guid PerformedBy { get; set; } // IdentityDB.users.id
    public DateTime PerformedAt { get; set; }
}
