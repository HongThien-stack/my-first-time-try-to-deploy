namespace IdentityService.Application.Interfaces;

/// <summary>
/// Contract for OTP generation.
/// </summary>
public interface IOtpService
{
    /// <summary>Generates a random 6-digit numeric OTP string.</summary>
    string GenerateOtp();
}
