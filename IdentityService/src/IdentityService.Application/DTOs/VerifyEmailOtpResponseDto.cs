namespace IdentityService.Application.DTOs;

/// <summary>
/// Response model after successfully verifying email OTP.
/// </summary>
public class VerifyEmailOtpResponseDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public bool IsEmailVerified { get; set; }
}
