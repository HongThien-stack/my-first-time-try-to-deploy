namespace InventoryService.Domain.Entities;

public class InventoryCheck
{
    public Guid Id { get; set; }
    public string CheckNumber { get; set; } = string.Empty; // IC-2024-001
    public string LocationType { get; set; } = string.Empty; // WAREHOUSE | STORE
    public Guid LocationId { get; set; }
    public string CheckType { get; set; } = string.Empty; // FULL | PARTIAL | SPOT
    public DateTime CheckDate { get; set; }
    public Guid CheckedBy { get; set; } // IdentityDB.users.id
    public string Status { get; set; } = "PENDING"; // PENDING | COMPLETED
    public int TotalDiscrepancies { get; set; } = 0;
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }

    // Navigation properties
    public ICollection<InventoryCheckItem> InventoryCheckItems { get; set; } = new List<InventoryCheckItem>();
}
