using IdentityService.Application.Interfaces;
using IdentityService.Domain.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace IdentityService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UtilityController : ControllerBase
{
    private static readonly HashSet<string> SampleUserEmails = new(StringComparer.OrdinalIgnoreCase)
    {
        "admin@company.com",
        "admin2@company.com",
        "whadmin1@company.com",
        "whadmin2@company.com",
        "whmanager1@company.com",
        "whmanager2@company.com",
        "whmanager3@company.com",
        "whstaff1@company.com",
        "whstaff2@company.com",
        "whstaff3@company.com",
        "manager1@company.com",
        "manager2@company.com",
        "storestaff1@company.com",
        "storestaff2@company.com",
        "storestaff3@company.com",
        "storestaff4@company.com",
        "customer1@gmail.com",
        "customer2@gmail.com",
        "customer3@gmail.com",
        "customer4@gmail.com",
        "inactive@company.com",
        "suspended@company.com"
    };

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
    /// Update sample user passwords only (for migration from old hash format)
    /// </summary>
    [HttpPost("update-passwords")]
    public async Task<IActionResult> UpdateAllPasswords()
    {
        try
        {
            var users = await _userRepository.GetAllAsync();
            int updatedCount = 0;
            int skippedCount = 0;

            foreach (var user in users)
            {
                if (!SampleUserEmails.Contains(user.Email))
                {
                    skippedCount++;
                    continue;
                }

                // Re-hash with new format using password "Password123!"
                user.PasswordHash = _passwordHasher.HashPassword("Password123!");
                await _userRepository.UpdateAsync(user);
                updatedCount++;
            }

            _logger.LogInformation("Updated passwords for {UpdatedCount} sample users, skipped {SkippedCount} non-sample users",
                updatedCount, skippedCount);

            return Ok(new
            {
                success = true,
                message = $"Successfully updated passwords for {updatedCount} sample users",
                updatedCount,
                skippedCount
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
