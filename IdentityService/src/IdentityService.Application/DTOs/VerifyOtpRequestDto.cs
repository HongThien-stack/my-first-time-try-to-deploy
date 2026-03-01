using System.ComponentModel.DataAnnotations;

namespace IdentityService.Application.DTOs;

/// <summary>
/// Request model for the second step of OTP-based registration:
/// the client submits the 6-digit OTP received by email together with
/// the full account details.  If the OTP is valid the account is created.
/// </summary>
public class VerifyOtpRequestDto
{
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    [MaxLength(255, ErrorMessage = "Email must not exceed 255 characters")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "OTP is required")]
    [StringLength(6, MinimumLength = 6, ErrorMessage = "OTP must be exactly 6 digits")]
    [RegularExpression(@"^\d{6}$", ErrorMessage = "OTP must contain only digits")]
    public string Otp { get; set; } = string.Empty;

    [Required(ErrorMessage = "Full name is required")]
    [MaxLength(255, ErrorMessage = "Full name must not exceed 255 characters")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required")]
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Confirm password is required")]
    [Compare(nameof(Password), ErrorMessage = "Passwords do not match")]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Phone(ErrorMessage = "Invalid phone number format")]
    [MaxLength(20, ErrorMessage = "Phone must not exceed 20 characters")]
    public string? Phone { get; set; }
}
