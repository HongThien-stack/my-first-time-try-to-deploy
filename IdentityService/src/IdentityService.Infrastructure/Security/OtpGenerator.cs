using IdentityService.Application.Interfaces;
using System.Security.Cryptography;

namespace IdentityService.Infrastructure.Security;

/// <summary>
/// Generates cryptographically random 6-digit OTP codes.
/// </summary>
public class OtpGenerator : IOtpService
{
    public string GenerateOtp()
    {
        // Use RandomNumberGenerator for cryptographic randomness (0–999999 → zero-padded to 6 digits)
        var randomNumber = RandomNumberGenerator.GetInt32(0, 1_000_000);
        return randomNumber.ToString("D6");
    }
}
