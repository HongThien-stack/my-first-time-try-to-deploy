namespace IdentityService.Domain.Entities;

public class UserLoginLog
{
    public long Id { get; set; }
    public Guid? UserId { get; set; } // Nullable to allow logging failed attempts when user not found
    public DateTime LoginAt { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public string Status { get; set; } = string.Empty; // SUCCESS | FAILED | BLOCKED
    public string? FailureReason { get; set; }

    // Navigation properties
    public virtual User? User { get; set; }
}
