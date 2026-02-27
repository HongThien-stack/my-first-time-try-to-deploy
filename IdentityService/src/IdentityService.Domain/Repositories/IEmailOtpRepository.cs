using IdentityService.Domain.Entities;

namespace IdentityService.Domain.Repositories;

public interface IEmailOtpRepository
{
    /// <summary>Persists a new OTP record.</summary>
    Task<EmailOtp> CreateAsync(EmailOtp otp);

    /// <summary>Returns the most recent (latest-created) OTP for the given email, or null if none.</summary>
    Task<EmailOtp?> GetLatestByEmailAsync(string email);

    /// <summary>Returns the most recent valid OTP for a given userId, or null if none.</summary>
    Task<EmailOtp?> GetLatestByUserIdAsync(Guid userId);

    /// <summary>Marks an existing OTP record as updated (IsUsed = true).</summary>
    Task UpdateAsync(EmailOtp otp);

    /// <summary>
    /// Marks all existing OTPs for the given email as used so only the newest one is valid.
    /// Call this before inserting a fresh OTP to prevent reuse of old codes.
    /// </summary>
    Task InvalidateAllByEmailAsync(string email);
}

