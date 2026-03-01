using System.ComponentModel.DataAnnotations;

namespace IdentityService.Application.DTOs;

/// <summary>
/// Request model for the first step of OTP-based registration:
/// the client sends only the email address, and the server generates
/// and emails a 6-digit OTP.
/// </summary>
public class SendOtpRequestDto
{
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    [MaxLength(255, ErrorMessage = "Email must not exceed 255 characters")]
    public string Email { get; set; } = string.Empty;
}
