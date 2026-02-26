namespace ProductService.Domain.Entities;

public class ProductAuditLog
{
    public long Id { get; set; }
    public Guid ProductId { get; set; }
    public Guid PerformedBy { get; set; } // User ID from Identity Service
    public string PerformedByName { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty; // CREATE | UPDATE | DELETE | ACTIVATE | DEACTIVATE
    public string? OldValues { get; set; } // JSON format
    public string? NewValues { get; set; } // JSON format
    public string? Description { get; set; }
    public string? IpAddress { get; set; }
    public DateTime PerformedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation property
    public Product? Product { get; set; }
}
