namespace IdentityService.Domain.Entities;

public class UserAuditLog
{
    public long Id { get; set; }
    public Guid UserId { get; set; } // User being modified
    public Guid PerformedBy { get; set; } // Admin/Manager who made the change
    public string Action { get; set; } = string.Empty; // CREATE | UPDATE | DELETE | ACTIVATE | DEACTIVATE | SUSPEND | RESET_PASSWORD | CHANGE_ROLE
    public string EntityType { get; set; } = "USER"; // USER | STAFF
    public string? OldValues { get; set; } // JSON format of old values
    public string? NewValues { get; set; } // JSON format of new values
    public string? Description { get; set; }
    public string? IpAddress { get; set; }
    public DateTime PerformedAt { get; set; }

    // Navigation properties
    public virtual User User { get; set; } = null!;
    public virtual User Performer { get; set; } = null!;
}
