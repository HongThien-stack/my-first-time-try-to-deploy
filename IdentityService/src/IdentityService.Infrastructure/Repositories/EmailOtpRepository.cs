using IdentityService.Domain.Entities;
using IdentityService.Domain.Repositories;
using IdentityService.Infrastructure.Data;

namespace IdentityService.Infrastructure.Repositories;

/// <summary>
/// Not used — OTP is stored directly in the users table (otp_code, otp_purpose, otp_expires_at).
/// Kept for interface compliance only.
/// </summary>
public class EmailOtpRepository : IEmailOtpRepository
{
    public EmailOtpRepository(IdentityDbContext context) { }

    public Task<EmailOtp> CreateAsync(EmailOtp otp) => throw new NotSupportedException();
    public Task<EmailOtp?> GetLatestByEmailAsync(string email) => throw new NotSupportedException();
    public Task<EmailOtp?> GetLatestByUserIdAsync(Guid userId) => throw new NotSupportedException();
    public Task UpdateAsync(EmailOtp otp) => throw new NotSupportedException();
    public Task InvalidateAllByEmailAsync(string email) => throw new NotSupportedException();
}
