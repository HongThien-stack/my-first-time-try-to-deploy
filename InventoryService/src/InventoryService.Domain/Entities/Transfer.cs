namespace InventoryService.Domain.Entities;

public class Transfer
{
    public Guid Id { get; set; }
    public string TransferNumber { get; set; } = string.Empty; // TRF-2024-001
    public string FromLocationType { get; set; } = string.Empty; // WAREHOUSE | STORE
    public Guid FromLocationId { get; set; }
    public string ToLocationType { get; set; } = string.Empty; // WAREHOUSE | STORE
    public Guid ToLocationId { get; set; }
    public DateTime TransferDate { get; set; }
    public DateTime? ExpectedDelivery { get; set; }
    public DateTime? ActualDelivery { get; set; }
    public string Status { get; set; } = "PENDING"; // PENDING | IN_TRANSIT | DELIVERED | CANCELLED
    public Guid? ShippedBy { get; set; } // IdentityDB.users.id
    public Guid? ReceivedBy { get; set; } // IdentityDB.users.id
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Guid? RestockRequestId { get; set; } // restock_requests.id

    // Navigation properties
    public ICollection<TransferItem> TransferItems { get; set; } = new List<TransferItem>();
    public ICollection<RestockRequest> RestockRequests { get; set; } = new List<RestockRequest>();
    public ICollection<StoreReceivingLog> StoreReceivingLogs { get; set; } = new List<StoreReceivingLog>();
}
