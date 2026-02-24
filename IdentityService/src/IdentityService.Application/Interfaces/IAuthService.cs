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
}
