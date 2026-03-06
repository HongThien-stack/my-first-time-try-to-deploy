using IdentityService.Application.DTOs;
using IdentityService.Application.Interfaces;
using IdentityService.Domain.Entities;
using IdentityService.Domain.Repositories;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityService.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtService _jwtService;
    private readonly IUserLoginLogRepository _loginLogRepository;
    private readonly IOtpService _otpService;
    private readonly IEmailService _emailService;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        IPasswordHasher passwordHasher,
        IJwtService jwtService,
        IUserLoginLogRepository loginLogRepository,
        IOtpService otpService,
        IEmailService emailService,
        ILogger<AuthService> logger = null)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _passwordHasher = passwordHasher;
        _jwtService = jwtService;
        _loginLogRepository = loginLogRepository;
        _otpService = otpService;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<RegisterResponseDto> RegisterAsync(RegisterRequestDto request)
    {
        // Check if email already exists
        if (await _userRepository.ExistsByEmailAsync(request.Email))
        {
            throw new InvalidOperationException("Email already exists");
        }
        // Check if phone already exists
        if (await _userRepository.ExistsByPhoneAsync(request.Phone))
        {
            throw new InvalidOperationException("Phone number already exists");
        }
        // Check if passwords match (additional server-side validation)
        if (request.Password != request.ConfirmPassword)
        {
            throw new InvalidOperationException("Passwords do not match");
        }
        // Create new user
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            FullName = request.FullName,
            Phone = request.Phone,
            PasswordHash = _passwordHasher.HashPassword(request.Password),
            RoleId = 6, // Default role: Customer
            Status = "INACTIVE",
            EmailVerified = false,
            OtpAttempts = 0
        };
        await _userRepository.CreateAsync(user);

        // Generate and send OTP for email verification
        await SendEmailOtpAsync(user);

        return new RegisterResponseDto
        {
            UserId = user.Id,
            Email = user.Email,
            FullName = user.FullName ?? string.Empty,
            Message = "Account created. Please verify your email in profile page."
        };
    }

    public async Task<LoginResponseDto> LoginAsync(LoginRequestDto request, string? ipAddress = null)
    {
        // Find user by email
        var user = await _userRepository.GetByEmailAsync(request.Email);

        if (user == null)
        {
            // Log failed login attempt - user not found
            await LogLoginAttemptAsync(null, "FAILED", "User not found", ipAddress, null);
            throw new UnauthorizedAccessException("Invalid email or password");
        }
        // Verify password
        if (!_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
        {
            // Log failed login attempt - invalid password
            await LogLoginAttemptAsync(user.Id, "FAILED", "Invalid password", ipAddress, null);
            throw new UnauthorizedAccessException("Invalid email or password");
        }
        // Check user status — only block SUSPENDED accounts; allow INACTIVE (email not yet verified)
        if (user.Status == "SUSPENDED")
        {
            await LogLoginAttemptAsync(user.Id, "BLOCKED", "Account is SUSPENDED", ipAddress, null);
            throw new UnauthorizedAccessException("Account is suspended. Please contact support.");
        }
        // Generate tokens
        var accessToken = _jwtService.GenerateAccessToken(user);
        var refreshToken = _jwtService.GenerateRefreshToken();
        var refreshTokenExpiry = _jwtService.GetRefreshTokenExpiryTime();
        // Update user with refresh token
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiresAt = refreshTokenExpiry;
        await _userRepository.UpdateAsync(user);
        // Log successful login
        await LogLoginAttemptAsync(user.Id, "SUCCESS", null, ipAddress, null);
        // Map to response DTO
        return new LoginResponseDto
        {
            AccessToken = accessToken,
            Email = user.Email,
            FullName = user.FullName,
            RoleId = user.RoleId,
            IsEmailVerified = user.EmailVerified,
            Message = user.EmailVerified ? null : "Email not verified. Please check your inbox and verify your email.",
            Workplace = user.WorkplaceId.HasValue && !string.IsNullOrEmpty(user.WorkplaceType)
                ? new WorkplaceDto
                {
                    Type = user.WorkplaceType,
                    Id = user.WorkplaceId.Value
                    // Name, Code, Address can be populated by querying InventoryDB if needed
                }
                : null
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
        // Clear refresh token
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
        // Check if refresh token is expired
        if (user.RefreshTokenExpiresAt == null || user.RefreshTokenExpiresAt < DateTime.UtcNow)
        {
            throw new UnauthorizedAccessException("Refresh token has expired");
        }
        // Check user status — only block SUSPENDED, allow INACTIVE (email not yet verified)
        if (user.Status == "SUSPENDED")
        {
            throw new UnauthorizedAccessException("Account is suspended. Please contact support.");
        }
        // Generate new tokens
        var newAccessToken = _jwtService.GenerateAccessToken(user);
        var newRefreshToken = _jwtService.GenerateRefreshToken();
        var newRefreshTokenExpiry = _jwtService.GetRefreshTokenExpiryTime();
        // Update user with new refresh token
        user.RefreshToken = newRefreshToken;
        user.RefreshTokenExpiresAt = newRefreshTokenExpiry;
        await _userRepository.UpdateAsync(user);
        return new LoginResponseDto
        {
            AccessToken = newAccessToken,
            Email = user.Email,
            FullName = user.FullName,
            RoleId = user.RoleId,
            IsEmailVerified = user.EmailVerified,
            Message = user.EmailVerified ? null : "Email not verified. Please check your inbox and verify your email.",
            Workplace = user.WorkplaceId.HasValue && !string.IsNullOrEmpty(user.WorkplaceType)
                ? new WorkplaceDto
                {
                    Type = user.WorkplaceType,
                    Id = user.WorkplaceId.Value
                    // Name, Code, Address can be populated by querying InventoryDB if needed
                }
                : null
        };
    }

    public async Task<UserDto> UpdateUserAsync(Guid id, UpdateUserRequestDto request)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null)
        {
            throw new InvalidOperationException("User not found");
        }
        // Update fields if provided
        if (!string.IsNullOrEmpty(request.FullName))
        {
            user.FullName = request.FullName;
        }
        if (!string.IsNullOrEmpty(request.Phone))
        {
            user.Phone = request.Phone;
        }
        if (!string.IsNullOrEmpty(request.Email))
        {
            if (string.IsNullOrWhiteSpace(request.Password))
            {
                throw new InvalidOperationException("Password cannot be empty");
            }

            user.PasswordHash = _passwordHasher.HashPassword(request.Password);
        }
        
        // Update workplace if provided
        // Allow clearing workplace by sending empty values
        if (request.WorkplaceType != null) // null means not updating, empty string means clearing
        {
            if (string.IsNullOrEmpty(request.WorkplaceType))
            {
                user.WorkplaceType = null;
                user.WorkplaceId = null;
            }
            else
            {
                if (!request.WorkplaceId.HasValue)
                {
                    throw new ArgumentException("WorkplaceId is required when WorkplaceType is specified");
                }
                user.WorkplaceType = request.WorkplaceType;
                user.WorkplaceId = request.WorkplaceId;
            }
        }
        
        await _userRepository.UpdateAsync(user);
        return MapToUserDto(user);
    }

    // CreateUserAsync (Admin tạo user nhân viên)
    public async Task<UserDto> CreateUserAsync(CreateUserRequestDto request)
    {
        // Check cơ bản thủ công (không dùng validator)
        if (string.IsNullOrWhiteSpace(request.FullName))
            throw new ArgumentException("Họ và tên không được để trống");

        if (string.IsNullOrWhiteSpace(request.Email))
            throw new ArgumentException("Email không được để trống");

        if (string.IsNullOrWhiteSpace(request.Password))
            throw new ArgumentException("Mật khẩu không được để trống");

        if (string.IsNullOrWhiteSpace(request.RoleName))
            throw new ArgumentException("Vai trò không được để trống");

        // Check email tồn tại
        if (await _userRepository.ExistsByEmailAsync(request.Email))
        {
            _logger?.LogWarning("Email {Email} already exists when creating user", request.Email);
            throw new InvalidOperationException("Email đã tồn tại trong hệ thống");
        }

        // Tìm role bằng tên
        var role = await _roleRepository.GetByNameAsync(request.RoleName);
        if (role == null)
        {
            _logger?.LogWarning("Role {RoleName} not found when creating user", request.RoleName);
            throw new InvalidOperationException($"Vai trò '{request.RoleName}' không tồn tại");
        }

        // Validate workplace assignment
        if (!string.IsNullOrEmpty(request.WorkplaceType) && !request.WorkplaceId.HasValue)
        {
            throw new ArgumentException("WorkplaceId is required when WorkplaceType is specified");
        }
        if (request.WorkplaceId.HasValue && string.IsNullOrEmpty(request.WorkplaceType))
        {
            throw new ArgumentException("WorkplaceType is required when WorkplaceId is specified");
        }

        // Hash password
        var passwordHash = _passwordHasher.HashPassword(request.Password);

        // Tạo user mới
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
            OtpAttempts = 0,
            WorkplaceType = request.WorkplaceType,
            WorkplaceId = request.WorkplaceId
            // OTP, RefreshToken giữ null
        };

        await _userRepository.CreateAsync(newUser);

        _logger?.LogInformation("Created new user: {Email} with role {RoleName}", request.Email, request.RoleName);

        return MapToUserDto(newUser);
    }

    public async Task<ForgotPasswordResponseDto> ForgotPasswordAsync(ForgotPasswordRequestDto request)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email);

        // Always return success to avoid user enumeration
        if (user == null)
        {
            return new ForgotPasswordResponseDto
            {
                Success = true,
                Message = "If the email exists, a password reset OTP has been sent."
            };
        }

        if (user.Status != "ACTIVE")
        {
            throw new InvalidOperationException($"Account is {user.Status.ToLower()}");
        }

        var otp = _otpService.GenerateOtp();

        user.OtpCode = otp;
        user.OtpPurpose = "PASSWORD_RESET";
        user.OtpExpiresAt = DateTime.UtcNow.AddMinutes(15);
        user.OtpAttempts = 0;

        await _userRepository.UpdateAsync(user);

        try
        {
            await _emailService.SendPasswordResetOtpEmailAsync(user.Email, user.FullName ?? user.Email, otp);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to send password reset OTP email to {Email}", user.Email);
            // Don't rethrow — OTP is stored, user can request again
        }

        return new ForgotPasswordResponseDto
        {
            Success = true,
            Message = "A password reset OTP has been sent to your email."
        };
    }

    public async Task<ResetPasswordResponseDto> ResetPasswordAsync(ResetPasswordRequestDto request)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email);

        if (user == null ||
            user.OtpCode != request.OTP ||
            user.OtpPurpose != "PASSWORD_RESET")
        {
            throw new UnauthorizedAccessException("Invalid or expired reset OTP");
        }

        if (user.OtpExpiresAt == null || user.OtpExpiresAt < DateTime.UtcNow)
        {
            user.OtpCode = null;
            user.OtpPurpose = null;
            user.OtpExpiresAt = null;
            await _userRepository.UpdateAsync(user);

            throw new UnauthorizedAccessException("Reset OTP has expired. Please request a new one.");
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
            Success = true,
            Message = "Password has been reset successfully. Please login with your new password."
        };
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
            },
            Workplace = user.WorkplaceId.HasValue && !string.IsNullOrEmpty(user.WorkplaceType)
                ? new WorkplaceDto
                {
                    Type = user.WorkplaceType,
                    Id = user.WorkplaceId.Value
                    // Name, Code, Address can be populated by querying InventoryDB if needed
                }
                : null
        };
    }

    private string GenerateRandomToken()
    {
        var random = new Random();
        return random.Next(100000, 999999).ToString();
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

        // Soft delete: chuyển status thành INACTIVE
        user.Status = "INACTIVE";
        await _userRepository.UpdateAsync(user);

        _logger?.LogInformation("User with ID {UserId} has been set to INACTIVE", id);

        return MapToUserDto(user);
    }

    // ── Email OTP ──────────────────────────────────────────────────────────────

    public async Task<VerifyEmailOtpResponseDto> VerifyEmailOtpAsync(Guid userId, VerifyEmailOtpRequestDto request)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            throw new InvalidOperationException("User not found");

        if (user.EmailVerified)
        {
            return new VerifyEmailOtpResponseDto
            {
                Success = true,
                Message = "Email is already verified.",
                IsEmailVerified = true
            };
        }

        if (string.IsNullOrEmpty(user.OtpCode))
            throw new InvalidOperationException("No OTP found. Please request a new OTP.");

        if (user.OtpPurpose != "EMAIL_VERIFICATION")
            throw new InvalidOperationException("Invalid OTP purpose. Please request a new OTP.");

        if (user.OtpExpiresAt == null || user.OtpExpiresAt < DateTime.UtcNow)
            throw new InvalidOperationException("OTP has expired. Please request a new OTP.");

        if (user.OtpCode?.Trim() != request.Otp.Trim())
            throw new InvalidOperationException("Invalid OTP code.");

        // Mark email as verified, activate account and clear OTP fields
        user.EmailVerified = true;
        user.Status = "ACTIVE";
        user.OtpCode = null;
        user.OtpPurpose = null;
        user.OtpExpiresAt = null;
        user.OtpAttempts = 0;
        await _userRepository.UpdateAsync(user);

        _logger?.LogInformation("Email verified successfully for user {UserId}", userId);

        return new VerifyEmailOtpResponseDto
        {
            Success = true,
            Message = "Email verified successfully.",
            IsEmailVerified = true
        };
    }

    public async Task ResendEmailOtpAsync(Guid userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            throw new InvalidOperationException("User not found");

        if (user.EmailVerified)
            throw new InvalidOperationException("Email is already verified.");

        await SendEmailOtpAsync(user);

        _logger?.LogInformation("OTP resent for user {UserId}", userId);
    }

    /// <summary>Generates a new OTP, saves it to user fields, and sends via email.</summary>
    private async Task SendEmailOtpAsync(User user)
    {
        var code = _otpService.GenerateOtp();

        user.OtpCode = code;
        user.OtpPurpose = "EMAIL_VERIFICATION";
        user.OtpExpiresAt = DateTime.UtcNow.AddMinutes(5);
        user.OtpAttempts = 0;
        await _userRepository.UpdateAsync(user);

        try
        {
            await _emailService.SendOtpEmailAsync(user.Email, user.FullName ?? user.Email, code);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to send OTP email to {Email}", user.Email);
            // Don't rethrow — OTP is stored, user can request resend
        }
    }

    private async Task LogLoginAttemptAsync(Guid? userId, string status, string? failureReason, string? ipAddress, string? userAgent)
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