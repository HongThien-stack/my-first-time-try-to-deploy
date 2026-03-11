namespace InventoryService.Domain.Entities;

public class RestockRequest
{
    public Guid Id { get; set; }
    public string RequestNumber { get; set; } = string.Empty; // RST-2024-001

    // Hierarchy flow:
    //   Store Manager     → from=branch_warehouse (WAREHOUSE), to=store (STORE)
    //   Warehouse Manager → from=kho_tong (WAREHOUSE),         to=branch (WAREHOUSE)
    //   Warehouse Admin   → from=null (external supplier),     to=kho_tong (WAREHOUSE)
    public Guid? FromWarehouseId { get; set; }      // Source of goods; null = external supplier
    public string FromLocationType { get; set; } = "WAREHOUSE"; // WAREHOUSE | STORE
    public Guid? ToWarehouseId { get; set; }        // Destination / requester's location
    public string ToLocationType { get; set; } = "WAREHOUSE";   // WAREHOUSE | STORE

    public Guid RequestedBy { get; set; } // IdentityDB.users.id
    public DateTime RequestedDate { get; set; }
    public string Priority { get; set; } = "NORMAL"; // NORMAL | HIGH | URGENT
    public string Status { get; set; } = "PENDING"; // PENDING | APPROVED | PROCESSING | COMPLETED | REJECTED
    public Guid? ApprovedBy { get; set; }
    public DateTime? ApprovedDate { get; set; }
    public Guid? TransferId { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public Transfer? Transfer { get; set; }
    public ICollection<RestockRequestItem> RestockRequestItems { get; set; } = new List<RestockRequestItem>();
}
