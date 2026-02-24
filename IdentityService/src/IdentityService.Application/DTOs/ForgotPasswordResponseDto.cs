namespace IdentityService.Application.DTOs;

public class ForgotPasswordResponseDto
{
    public string Message { get; set; } = string.Empty;
    public string? ResetToken { get; set; } // For testing purposes only, remove in production
}
