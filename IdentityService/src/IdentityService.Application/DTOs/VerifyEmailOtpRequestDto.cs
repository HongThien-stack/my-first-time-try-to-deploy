using System.ComponentModel.DataAnnotations;

namespace IdentityService.Application.DTOs;

/// <summary>
/// Request model for verifying email OTP.
/// The user is already authenticated (JWT), so only the OTP code is needed.
/// </summary>
public class VerifyEmailOtpRequestDto
{
    [Required(ErrorMessage = "OTP is required")]
    [StringLength(6, MinimumLength = 6, ErrorMessage = "OTP must be exactly 6 digits")]
    [RegularExpression(@"^\d{6}$", ErrorMessage = "OTP must contain only digits")]
    public string Otp { get; set; } = string.Empty;
}
