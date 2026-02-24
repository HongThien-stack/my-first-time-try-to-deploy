using IdentityService.Application.DTOs;
using IdentityService.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IdentityService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    /// <summary>
    /// Register new user account
    /// </summary>
    /// <param name="request">Registration information</param>
    /// <returns>User registration confirmation</returns>
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
    {
        try
        {
            var response = await _authService.RegisterAsync(request);
            
            _logger.LogInformation("User {Email} registered successfully", request.Email);
            
            return Ok(new
            {
                success = true,
                message = response.Message,
                data = response
            });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Registration failed for {Email}: {Message}", request.Email, ex.Message);
            return BadRequest(new
            {
                success = false,
                message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during registration for {Email}", request.Email);
            return StatusCode(500, new
            {
                success = false,
                message = "An error occurred during registration"
            });
        }
    }

    /// <summary>
    /// Login endpoint - authenticates user and returns JWT tokens
    /// </summary>
    /// <param name="request">Login credentials</param>
    /// <returns>Access token, refresh token, and user information</returns>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        try
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            var response = await _authService.LoginAsync(request, ipAddress);
            
            _logger.LogInformation("User {Email} logged in successfully", request.Email);
            
            return Ok(new
            {
                success = true,
                message = "Login successful",
                data = response
            });
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("Failed login attempt for {Email}: {Message}", request.Email, ex.Message);
            return Unauthorized(new
            {
                success = false,
                message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for {Email}", request.Email);
            return StatusCode(500, new
            {
                success = false,
                message = "An error occurred during login"
            });
        }
    }

    /// <summary>
    /// Logout endpoint - invalidates the refresh token
    /// </summary>
    /// <param name="request">Refresh token to invalidate</param>
    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] RefreshTokenRequestDto request)
    {
        try
        {
            await _authService.LogoutAsync(request.RefreshToken);
            
            _logger.LogInformation("User logged out successfully");
            
            return Ok(new
            {
                success = true,
                message = "Logout successful"
            });
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("Failed logout attempt: {Message}", ex.Message);
            return Unauthorized(new
            {
                success = false,
                message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during logout");
            return StatusCode(500, new
            {
                success = false,
                message = "An error occurred during logout"
            });
        }
    }

    /// <summary>
    /// Refresh token endpoint - generates new access token using refresh token
    /// </summary>
    /// <param name="request">Current refresh token</param>
    /// <returns>New access token and refresh token</returns>
    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDto request)
    {
        try
        {
            var response = await _authService.RefreshTokenAsync(request.RefreshToken);
            
            _logger.LogInformation("Token refreshed successfully");
            
            return Ok(new
            {
                success = true,
                message = "Token refreshed successfully",
                data = response
            });
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("Failed token refresh: {Message}", ex.Message);
            return Unauthorized(new
            {
                success = false,
                message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during token refresh");
            return StatusCode(500, new
            {
                success = false,
                message = "An error occurred during token refresh"
            });
        }
    }

    /// <summary>
    /// Forgot password endpoint - sends reset token to user email
    /// </summary>
    /// <param name="request">User email address</param>
    /// <returns>Password reset link and token</returns>
    [HttpPost("forgot-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDto request)
    {
        try
        {
            var response = await _authService.ForgotPasswordAsync(request);
            
            _logger.LogInformation("Forgot password requested for {Email}", request.Email);
            
            return Ok(new
            {
                success = response.Success,
                message = response.Message,
                data = new { resetToken = response.ResetToken }
            });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Forgot password failed for {Email}: {Message}", request.Email, ex.Message);
            return BadRequest(new
            {
                success = false,
                message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during forgot password for {Email}", request.Email);
            return StatusCode(500, new
            {
                success = false,
                message = "An error occurred during password reset request"
            });
        }
    }

    /// <summary>
    /// Reset password endpoint - resets user password using token
    /// </summary>
    /// <param name="request">Email, reset token, and new password</param>
    /// <returns>Password reset confirmation</returns>
    [HttpPost("reset-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestDto request)
    {
        try
        {
            var response = await _authService.ResetPasswordAsync(request);
            
            _logger.LogInformation("Password reset successfully for {Email}", request.Email);
            
            return Ok(new
            {
                success = response.Success,
                message = response.Message
            });
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("Reset password failed for {Email}: {Message}", request.Email, ex.Message);
            return Unauthorized(new
            {
                success = false,
                message = ex.Message
            });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Reset password validation failed for {Email}: {Message}", request.Email, ex.Message);
            return BadRequest(new
            {
                success = false,
                message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during password reset for {Email}", request.Email);
            return StatusCode(500, new
            {
                success = false,
                message = "An error occurred during password reset"
            });
        }
    }

    /// <summary>
    /// Test endpoint to verify authentication
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    public IActionResult GetCurrentUser()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
        var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

        return Ok(new
        {
            success = true,
            data = new
            {
                userId,
                email,
                role,
                claims = User.Claims.Select(c => new { c.Type, c.Value })
            }
        });
    }
}
