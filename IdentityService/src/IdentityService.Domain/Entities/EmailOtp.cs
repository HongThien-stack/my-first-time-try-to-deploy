namespace IdentityService.Domain.Entities;

/// <summary>
/// OTP record for email verification linked to an existing user account.
/// </summary>
public class EmailOtp
{
    public long Id { get; set; }

    /// <summary>The user this OTP belongs to.</summary>
    public Guid UserId { get; set; }

    /// <summary>The email address this OTP was sent to.</summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>The plain 6-digit OTP code (stored temporarily, not hashed for simplicity).</summary>
    public string OtpCode { get; set; } = string.Empty;

    /// <summary>UTC time when this OTP expires (5 minutes after creation).</summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>Whether this OTP has already been used.</summary>
    public bool IsUsed { get; set; }

    /// <summary>UTC time when this OTP was created.</summary>
    public DateTime CreatedAt { get; set; }

    // Navigation property
    public virtual User User { get; set; } = null!;
}
