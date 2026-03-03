namespace InventoryService.Domain.Entities;

public class DamageReport
{
    public Guid Id { get; set; }
    public string ReportNumber { get; set; } = string.Empty; // DMG-2024-001
    public string LocationType { get; set; } = string.Empty; // WAREHOUSE | STORE
    public Guid LocationId { get; set; }
    public string DamageType { get; set; } = string.Empty; // EXPIRED | PHYSICAL_DAMAGE | QUALITY_ISSUE | THEFT | OTHER
    public Guid ReportedBy { get; set; } // IdentityDB.users.id
    public DateTime ReportedDate { get; set; }
    public decimal? TotalValue { get; set; }
    public string? Description { get; set; }
    public string? Photos { get; set; } // JSON array of image URLs
    public string Status { get; set; } = "PENDING"; // PENDING | APPROVED | REJECTED
    public Guid? ApprovedBy { get; set; } // IdentityDB.users.id
    public DateTime? ApprovedDate { get; set; }
    public DateTime CreatedAt { get; set; }
}
