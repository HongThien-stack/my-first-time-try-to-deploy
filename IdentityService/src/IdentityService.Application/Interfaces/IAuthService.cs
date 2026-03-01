using IdentityService.Application.DTOs;

namespace IdentityService.Application.Interfaces;

public interface IAuthService
{
    Task<RegisterResponseDto> RegisterAsync(RegisterRequestDto request);
    Task<LoginResponseDto> LoginAsync(LoginRequestDto request, string? ipAddress = null);
    Task LogoutAsync(string refreshToken);
    Task<LoginResponseDto> RefreshTokenAsync(string refreshToken);
    Task<UserDto> UpdateUserAsync(Guid id, UpdateUserRequestDto request);
    Task<UserDto> CreateUserAsync(CreateUserRequestDto request);
    Task<List<UserDto>> GetAllUsersAsync();
    Task<UserDto> DeleteUserAsync(Guid id);
    Task<ForgotPasswordResponseDto> ForgotPasswordAsync(ForgotPasswordRequestDto request);
    Task<ResetPasswordResponseDto> ResetPasswordAsync(ResetPasswordRequestDto request);
    /// <summary>
    /// Verifies the email OTP submitted by an authenticated user.
    /// On success sets IsEmailVerified = true.
    /// </summary>
    Task<VerifyEmailOtpResponseDto> VerifyEmailOtpAsync(Guid userId, VerifyEmailOtpRequestDto request);

    /// <summary>
    /// Re-sends a fresh OTP to the authenticated user's email.
    /// </summary>
    Task ResendEmailOtpAsync(Guid userId);
}

