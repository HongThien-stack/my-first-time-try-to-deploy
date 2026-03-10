namespace InventoryService.Domain.Entities;

public class StockMovement
{
    public Guid Id { get; set; }
    public string MovementNumber { get; set; } = string.Empty; // SM-2024-001
    public string MovementType { get; set; } = string.Empty; // INBOUND | OUTBOUND | TRANSFER | ADJUSTMENT
    public Guid LocationId { get; set; } // warehouse_id or store_id
    public string LocationType { get; set; } = string.Empty; // WAREHOUSE | STORE
    public DateTime MovementDate { get; set; }
    public string? Supplier { get; set; } // For INBOUND
    public Guid? PurchaseOrderId { get; set; } // Purchase Order number
    public Guid? TransferId { get; set; }
    public Guid? ReceivedBy { get; set; } // IdentityDB.users.id
    public string Status { get; set; } = "COMPLETED"; // PENDING | COMPLETED | CANCELLED
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }

    // Navigation properties
    public ICollection<StockMovementItem> StockMovementItems { get; set; } = new List<StockMovementItem>();
}
