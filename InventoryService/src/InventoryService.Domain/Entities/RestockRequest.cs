namespace InventoryService.Domain.Entities;

public class RestockRequest
{
    public Guid Id { get; set; }
    public string RequestNumber { get; set; } = string.Empty; // RST-2024-001
    public Guid StoreId { get; set; } // Store requesting restock
    public Guid? WarehouseId { get; set; } // Warehouse to fulfill
    public Guid RequestedBy { get; set; } // IdentityDB.users.id (Store staff)
    public DateTime RequestedDate { get; set; }
    public string Priority { get; set; } = "NORMAL"; // NORMAL | HIGH | URGENT
    public string Status { get; set; } = "PENDING"; // PENDING | APPROVED | PROCESSING | COMPLETED | REJECTED
    public Guid? ApprovedBy { get; set; } // IdentityDB.users.id (Warehouse manager)
    public DateTime? ApprovedDate { get; set; }
    public Guid? TransferId { get; set; } // Link to created transfer
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public Transfer? Transfer { get; set; }
    public ICollection<RestockRequestItem> RestockRequestItems { get; set; } = new List<RestockRequestItem>();
}
