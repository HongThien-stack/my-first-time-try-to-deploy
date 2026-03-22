using Microsoft.AspNetCore.Http;

namespace InventoryService.Application.DTOs;

public class DamageReportDto
{
    public Guid? Id { get; set; }
    public string? ReportNumber { get; set; }
    public string LocationType { get; set; } = string.Empty; // WAREHOUSE | STORE
    public Guid LocationId { get; set; }
    public Guid ProductId { get; set; }
    public string DamageType { get; set; } = string.Empty; // EXPIRED | PHYSICAL_DAMAGE | QUALITY_ISSUE | THEFT | OTHER
    public Guid ReportedBy { get; set; }
    public DateTime ReportedDate { get; set; }
    public int? Quality { get; set; } // Quality rating or condition (0-100 scale)
    public string? Description { get; set; }
    public List<string>? Photos { get; set; } // List of URLs (output only)
    public string? Status { get; set; } // PENDING | APPROVED | REJECTED
    public Guid? ApprovedBy { get; set; }
    public DateTime? ApprovedDate { get; set; }
    public DateTime? CreatedAt { get; set; }
}

public class CreateDamageReportRequest
{
    public string LocationType { get; set; } = string.Empty; // WAREHOUSE | STORE
    public Guid LocationId { get; set; }
    public Guid ProductId { get; set; }
    public string DamageType { get; set; } = string.Empty; // EXPIRED | PHYSICAL_DAMAGE | QUALITY_ISSUE | THEFT | OTHER
    public DateTime ReportedDate { get; set; }
    public int? Quality { get; set; } // Quality rating or condition (0-100 scale)
    public string? Description { get; set; }
    public List<IFormFile>? Photos { get; set; } // Upload photos
    // ReportedBy will be automatically set from current user JWT token
}

public class ApproveDamageReportRequest
{
    // ApprovedBy will be automatically set from current user JWT token
}

public class DamageReportListDto
{
    public Guid Id { get; set; }
    public string ReportNumber { get; set; } = string.Empty;
    public string LocationType { get; set; } = string.Empty;
    public Guid LocationId { get; set; }
    public Guid ProductId { get; set; }
    public string DamageType { get; set; } = string.Empty;
    public Guid ReportedBy { get; set; }
    public DateTime ReportedDate { get; set; }
    public int? Quality { get; set; }
    public string Status { get; set; } = "PENDING";
    public DateTime CreatedAt { get; set; }
}
