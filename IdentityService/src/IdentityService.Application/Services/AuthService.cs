using IdentityService.Application.DTOs;
using IdentityService.Application.Interfaces;
using IdentityService.Domain.Entities;
using IdentityService.Domain.Repositories;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace IdentityService.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtService _jwtService;
    private readonly IUserLoginLogRepository _loginLogRepository;
    private readonly ILogger<AuthService>? _logger;

    public AuthService(
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        IPasswordHasher passwordHasher,
        IJwtService jwtService,
        IUserLoginLogRepository loginLogRepository,
        ILogger<AuthService>? logger = null)  
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _passwordHasher = passwordHasher;
        _jwtService = jwtService;
        _loginLogRepository = loginLogRepository;
        _logger = logger;
    }

    public async Task<RegisterResponseDto> RegisterAsync(RegisterRequestDto request)
    {
        if (await _userRepository.ExistsByEmailAsync(request.Email))
        {
            throw new InvalidOperationException("Email already exists");
        }
        
        if (request.Password != request.ConfirmPassword)
        {
            throw new InvalidOperationException("Passwords do not match");
        }
        
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            FullName = request.FullName,
            Phone = request.Phone,
            PasswordHash = _passwordHasher.HashPassword(request.Password),
            RoleId = 5,
            Status = "ACTIVE",
            EmailVerified = false,
            OtpAttempts = 0
        };
        
        await _userRepository.CreateAsync(user);
        
        return new RegisterResponseDto
        {
            UserId = user.Id,
            Email = user.Email,
            FullName = user.FullName ?? string.Empty,
            Message = "Registration successful. Please verify your email."
        };
    }

    public async Task<LoginResponseDto> LoginAsync(LoginRequestDto request, string? ipAddress = null)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email);

        if (user == null)
        {
            await LogLoginAttemptAsync(Guid.Empty, "FAILED", "User not found", ipAddress, null);
            throw new UnauthorizedAccessException("Invalid email or password");
        }
        
        if (!_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
        {
            await LogLoginAttemptAsync(user.Id, "FAILED", "Invalid password", ipAddress, null);
            throw new UnauthorizedAccessException("Invalid email or password");
        }
        
        if (user.Status != "ACTIVE")
        {
            await LogLoginAttemptAsync(user.Id, "BLOCKED", $"Account is {user.Status}", ipAddress, null);
            throw new UnauthorizedAccessException($"Account is {user.Status.ToLower()}");
        }
        
        var accessToken = _jwtService.GenerateAccessToken(user);
        var refreshToken = _jwtService.GenerateRefreshToken();
        var refreshTokenExpiry = _jwtService.GetRefreshTokenExpiryTime();
        
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiresAt = refreshTokenExpiry;
        await _userRepository.UpdateAsync(user);
        
        await LogLoginAttemptAsync(user.Id, "SUCCESS", null, ipAddress, null);
        
        return new LoginResponseDto
        {
            AccessToken = accessToken,
            Email = user.Email,
            FullName = user.FullName,
            RoleId = user.RoleId
        };
    }

    public async Task LogoutAsync(string refreshToken)
    {
        if (string.IsNullOrEmpty(refreshToken))
        {
            throw new ArgumentException("Refresh token is required");
        }
        
        var user = await _userRepository.GetByRefreshTokenAsync(refreshToken);
        if (user == null)
        {
            throw new UnauthorizedAccessException("Invalid refresh token");
        }
        
        user.RefreshToken = null;
        user.RefreshTokenExpiresAt = null;
        await _userRepository.UpdateAsync(user);
    }

    public async Task<LoginResponseDto> RefreshTokenAsync(string refreshToken)
    {
        if (string.IsNullOrEmpty(refreshToken))
        {
            throw new ArgumentException("Refresh token is required");
        }
        
        var user = await _userRepository.GetByRefreshTokenAsync(refreshToken);
        if (user == null)
        {
            throw new UnauthorizedAccessException("Invalid refresh token");
        }
        
        if (user.RefreshTokenExpiresAt == null || user.RefreshTokenExpiresAt < DateTime.UtcNow)
        {
            throw new UnauthorizedAccessException("Refresh token has expired");
        }
        
        if (user.Status != "ACTIVE")
        {
            throw new UnauthorizedAccessException($"Account is {user.Status.ToLower()}");
        }
        
        var newAccessToken = _jwtService.GenerateAccessToken(user);
        var newRefreshToken = _jwtService.GenerateRefreshToken();
        var newRefreshTokenExpiry = _jwtService.GetRefreshTokenExpiryTime();
        
        user.RefreshToken = newRefreshToken;
        user.RefreshTokenExpiresAt = newRefreshTokenExpiry;
        await _userRepository.UpdateAsync(user);
        
        return new LoginResponseDto
        {
            AccessToken = newAccessToken,
            Email = user.Email,
            FullName = user.FullName,
            RoleId = user.RoleId
        };
    }

    public async Task<UserDto> UpdateUserAsync(Guid id, UpdateUserRequestDto request)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null)
        {
            throw new InvalidOperationException("User not found");
        }
        
        if (!string.IsNullOrEmpty(request.FullName))
        {
            user.FullName = request.FullName;
        }
        
        if (!string.IsNullOrEmpty(request.Phone))
        {
            user.Phone = request.Phone;
        }
        
        if (!string.IsNullOrEmpty(request.Password))
        {
            user.PasswordHash = _passwordHasher.HashPassword(request.Password);
        }
        
        await _userRepository.UpdateAsync(user);
        return MapToUserDto(user);
    }

    public async Task<UserDto> CreateUserAsync(CreateUserRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.FullName))
            throw new ArgumentException("Họ và tên không được để trống");

        if (string.IsNullOrWhiteSpace(request.Email))
            throw new ArgumentException("Email không được để trống");

        if (string.IsNullOrWhiteSpace(request.Password))
            throw new ArgumentException("Mật khẩu không được để trống");

        if (string.IsNullOrWhiteSpace(request.RoleName))
            throw new ArgumentException("Vai trò không được để trống");

        if (await _userRepository.ExistsByEmailAsync(request.Email))
        {
            _logger?.LogWarning("Email {Email} already exists when creating user", request.Email);
            throw new InvalidOperationException("Email đã tồn tại trong hệ thống");
        }

        var role = await _roleRepository.GetByNameAsync(request.RoleName);
        if (role == null)
        {
            _logger?.LogWarning("Role {RoleName} not found when creating user", request.RoleName);
            throw new InvalidOperationException($"Vai trò '{request.RoleName}' không tồn tại");
        }

        var passwordHash = _passwordHasher.HashPassword(request.Password);

        var newUser = new User
        {
            Id = Guid.NewGuid(),
            FullName = request.FullName,
            Email = request.Email,
            PasswordHash = passwordHash,
            Phone = request.Phone,
            RoleId = role.Id,
            Status = "ACTIVE",
            EmailVerified = false,
            OtpAttempts = 0
        };

        await _userRepository.CreateAsync(newUser);

        _logger?.LogInformation("Created new user: {Email} with role {RoleName}", request.Email, request.RoleName);

        return MapToUserDto(newUser);
    }

    public async Task<ForgotPasswordResponseDto> ForgotPasswordAsync(ForgotPasswordRequestDto request)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email);
        
        if (user == null)
        {
            return new ForgotPasswordResponseDto
            {
                Message = "If the email exists, a password reset token has been sent."
            };
        }

        if (user.Status != "ACTIVE")
        {
            throw new InvalidOperationException($"Account is {user.Status.ToLower()}");
        }

        var resetToken = GenerateRandomToken();
        
        user.OtpCode = resetToken;
        user.OtpPurpose = "PASSWORD_RESET";
        user.OtpExpiresAt = DateTime.UtcNow.AddMinutes(15);
        user.OtpAttempts = 0;

        await _userRepository.UpdateAsync(user);

        return new ForgotPasswordResponseDto
        {
            Message = "Password reset token has been sent to your email.",
            ResetToken = resetToken
        };
    }

    public async Task<ResetPasswordResponseDto> ResetPasswordAsync(ResetPasswordRequestDto request)
    {
        var users = await _userRepository.GetAllAsync();
        var user = users.FirstOrDefault(u => 
            u.OtpCode == request.Token && 
            u.OtpPurpose == "PASSWORD_RESET");

        if (user == null)
        {
            throw new UnauthorizedAccessException("Invalid or expired reset token");
        }

        if (user.OtpExpiresAt == null || user.OtpExpiresAt < DateTime.UtcNow)
        {
            user.OtpCode = null;
            user.OtpPurpose = null;
            user.OtpExpiresAt = null;
            await _userRepository.UpdateAsync(user);
            
            throw new UnauthorizedAccessException("Reset token has expired");
        }

        if (request.NewPassword != request.ConfirmPassword)
        {
            throw new InvalidOperationException("Passwords do not match");
        }

        user.PasswordHash = _passwordHasher.HashPassword(request.NewPassword);
        user.OtpCode = null;
        user.OtpPurpose = null;
        user.OtpExpiresAt = null;
        user.OtpAttempts = 0;
        user.RefreshToken = null;
        user.RefreshTokenExpiresAt = null;

        await _userRepository.UpdateAsync(user);

        return new ResetPasswordResponseDto
        {
            Message = "Password has been reset successfully. Please login with your new password."
        };
    }

    public async Task<List<UserDto>> GetAllUsersAsync()
    {
        var users = await _userRepository.GetAllAsync();  
        var userDtos = users.Select(user => MapToUserDto(user)).ToList();
        _logger?.LogInformation("Retrieved {Count} users", userDtos.Count);
        return userDtos;
    }

    public async Task<UserDto> DeleteUserAsync(Guid id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null)
        {
            _logger?.LogWarning("User with ID {UserId} not found for deletion", id);
            throw new InvalidOperationException("User not found");
        }

        user.Status = "INACTIVE";
        await _userRepository.UpdateAsync(user);

        _logger?.LogInformation("User with ID {UserId} has been set to INACTIVE", id);

        return MapToUserDto(user);
    }

    private string GenerateRandomToken()
    {
        var random = new Random();
        return random.Next(100000, 999999).ToString();
    }

    private UserDto MapToUserDto(User user)
    {
        return new UserDto
        {
            Id = user.Id,
            Email = user.Email,
            FullName = user.FullName,
            Phone = user.Phone,
            Status = user.Status,
            EmailVerified = user.EmailVerified,
            Role = new RoleDto
            {
                Id = user.Role?.Id ?? 0,
                Name = user.Role?.Name ?? "Unknown",
                Description = user.Role?.Description
            }
        };
    }

    private async Task LogLoginAttemptAsync(Guid userId, string status, string? failureReason, string? ipAddress, string? userAgent)
    {
        try
        {
            var loginLog = new UserLoginLog
            {
                UserId = userId,
                LoginAt = DateTime.UtcNow,
                IpAddress = ipAddress,
                UserAgent = userAgent,
                Status = status,
                FailureReason = failureReason
            };
            await _loginLogRepository.CreateAsync(loginLog);
        }
        catch
        {
            // Logging should not break the flow
        }
    }
}