namespace InventoryService.Application.DTOs;

public class DamageReportDto
{
    public Guid Id { get; set; }
    public string ReportNumber { get; set; } = string.Empty;
    public string LocationType { get; set; } = string.Empty; // WAREHOUSE | STORE
    public Guid LocationId { get; set; }
    public string DamageType { get; set; } = string.Empty; // EXPIRED | PHYSICAL_DAMAGE | QUALITY_ISSUE | THEFT | OTHER
    public Guid ReportedBy { get; set; }
    public DateTime ReportedDate { get; set; }
    public decimal? TotalValue { get; set; }
    public string? Description { get; set; }
    public string? Photos { get; set; } // JSON array of image URLs
    public string Status { get; set; } = "PENDING"; // PENDING | APPROVED | REJECTED
    public Guid? ApprovedBy { get; set; }
    public DateTime? ApprovedDate { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class DamageReportListDto
{
    public Guid Id { get; set; }
    public string ReportNumber { get; set; } = string.Empty;
    public string LocationType { get; set; } = string.Empty;
    public Guid LocationId { get; set; }
    public string DamageType { get; set; } = string.Empty;
    public Guid ReportedBy { get; set; }
    public DateTime ReportedDate { get; set; }
    public decimal? TotalValue { get; set; }
    public string Status { get; set; } = "PENDING";
    public DateTime CreatedAt { get; set; }
}

