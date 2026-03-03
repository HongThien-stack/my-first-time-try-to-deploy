namespace InventoryService.Domain.Entities;

public class StoreReceivingLog
{
    public Guid Id { get; set; }
    public Guid TransferId { get; set; }
    public Guid StoreId { get; set; }
    public Guid ReceivedBy { get; set; } // IdentityDB.users.id
    public DateTime ReceivedDate { get; set; }
    public string ConditionStatus { get; set; } = string.Empty; // GOOD | DAMAGED | PARTIAL
    public string? Notes { get; set; }
    public string? Photos { get; set; } // JSON array of image URLs

    // Navigation properties
    public Transfer Transfer { get; set; } = null!;
}
