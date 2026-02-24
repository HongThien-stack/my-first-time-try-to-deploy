using IdentityService.Application.Interfaces;
using IdentityService.Domain.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace IdentityService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UtilityController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ILogger<UtilityController> _logger;

    public UtilityController(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        ILogger<UtilityController> logger)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _logger = logger;
    }

    /// <summary>
    /// Update all user passwords with new hash (for migration from old hash format)
    /// </summary>
    [HttpPost("update-passwords")]
    public async Task<IActionResult> UpdateAllPasswords()
    {
        try
        {
            var users = await _userRepository.GetAllAsync();
            int updatedCount = 0;

            foreach (var user in users)
            {
                // Re-hash with new format using password "Password123!"
                user.PasswordHash = _passwordHasher.HashPassword("Password123!");
                await _userRepository.UpdateAsync(user);
                updatedCount++;
            }

            _logger.LogInformation("Updated passwords for {Count} users", updatedCount);

            return Ok(new
            {
                success = true,
                message = $"Successfully updated passwords for {updatedCount} users",
                count = updatedCount
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating passwords");
            return StatusCode(500, new
            {
                success = false,
                message = "Error updating passwords: " + ex.Message
            });
        }
    }

    /// <summary>
    /// Test database connection
    /// </summary>
    [HttpGet("test-db")]
    public async Task<IActionResult> TestDatabase()
    {
        try
        {
            var users = await _userRepository.GetAllAsync();
            return Ok(new
            {
                success = true,
                message = "Database connection successful",
                userCount = users.Count()
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database connection failed");
            return StatusCode(500, new
            {
                success = false,
                message = "Database connection failed: " + ex.Message
            });
        }
    }
}
