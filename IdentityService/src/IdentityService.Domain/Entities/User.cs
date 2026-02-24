namespace IdentityService.Domain.Entities;

public class User
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string? FullName { get; set; }
    public string? Phone { get; set; }
    public int RoleId { get; set; }
    public string Status { get; set; } = "ACTIVE"; // ACTIVE | INACTIVE | SUSPENDED

    // JWT Refresh Token
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiresAt { get; set; }

    // OTP for email verification / password reset
    public string? OtpCode { get; set; }
    public string? OtpPurpose { get; set; } // EMAIL_VERIFICATION | PASSWORD_RESET | TWO_FACTOR_AUTH
    public DateTime? OtpExpiresAt { get; set; }
    public int OtpAttempts { get; set; }

    // Email verification
    public bool EmailVerified { get; set; }

    // Navigation properties
    public virtual Role Role { get; set; } = null!;
    public virtual ICollection<UserLoginLog> LoginLogs { get; set; } = new List<UserLoginLog>();
    public virtual ICollection<UserAuditLog> AuditLogsAsUser { get; set; } = new List<UserAuditLog>();
    public virtual ICollection<UserAuditLog> AuditLogsAsPerformer { get; set; } = new List<UserAuditLog>();
}
