using IdentityService.Application.DTOs;
using IdentityService.Application.Interfaces;
using IdentityService.Domain.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IdentityService.API.Controllers;

[ApiController]
[Route("api/users")]
[Tags("User Management")]
[ Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly IAuthService _authService;
    private readonly ILogger<UsersController> _logger;

    public UsersController(
        IUserRepository userRepository,
        IAuthService authService,
        ILogger<UsersController> logger)
    {
        _userRepository = userRepository;
        _authService = authService;
        _logger = logger;
    }

    /// <summary>
    /// Get all users list (Admin only)
    /// </summary>
    /// <returns>List of all users</returns>
    [HttpGet("list")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetUsers()
    {
        try
        {
            var users = await _authService.GetAllUsersAsync();

            _logger.LogInformation("Retrieved {Count} users", users.Count);

            return Ok(new
            {
                success = true,
                message = "Users retrieved successfully",
                data = users
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all users");
            return StatusCode(500, new
            {
                success = false,
                message = "Error retrieving users: " + ex.Message
            });
        }
    }

    /// <summary>
    /// Get specific user details by ID
    /// </summary>
    /// <param name="id">User ID</param>
    /// <returns>User details</returns>
    [HttpGet("details/{id}")]
    public async Task<IActionResult> GetUser(Guid id)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(id);

            if (user == null)
            {
                _logger.LogWarning("User with ID {UserId} not found", id);
                return NotFound(new
                {
                    success = false,
                    message = "User not found"
                });
            }

            var userDto = new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                Phone = user.Phone,
                Status = user.Status,
                EmailVerified = user.EmailVerified,
                Role = new RoleDto { Id = user.RoleId, Name = user.Role?.Name ?? "Unknown" }
            };

            _logger.LogInformation("Retrieved user details for ID {UserId}", id);

            return Ok(new
            {
                success = true,
                message = "User details retrieved successfully",
                data = userDto
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user with ID {UserId}", id);
            return StatusCode(500, new
            {
                success = false,
                message = "Error retrieving user details: " + ex.Message
            });
        }
    }

    /// <summary>
    /// Create a new user (Admin only)
    /// </summary>
    /// <param name="request">User creation request</param>
    /// <returns>Created user details</returns>
    [HttpPost("create")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequestDto request)
    {
        try
        {
            if (request == null)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Request body cannot be empty"
                });
            }

            var createdUser = await _authService.CreateUserAsync(request);

            _logger.LogInformation("Created new user with ID {UserId}", createdUser.Id);

            return CreatedAtAction(nameof(GetUser), new { id = createdUser.Id }, new
            {
                success = true,
                message = "User created successfully",
                data = createdUser
            });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid input when creating user");
            return BadRequest(new
            {
                success = false,
                message = ex.Message
            });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation when creating user");
            return BadRequest(new
            {
                success = false,
                message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user");
            return StatusCode(500, new
            {
                success = false,
                message = "Error creating user: " + ex.Message
            });
        }
    }

    /// <summary>
    /// Update user details
    /// </summary>
    /// <param name="id">User ID</param>
    /// <param name="request">Update user request</param>
    /// <returns>Updated user details</returns>
    [HttpPut("update/{id}")]
    public async Task<IActionResult> UpdateUser(Guid id, [FromBody] UpdateUserRequestDto request)
    {
        try
        {
            if (request == null)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Request body cannot be empty"
                });
            }

            var updatedUser = await _authService.UpdateUserAsync(id, request);

            _logger.LogInformation("Updated user with ID {UserId}", id);

            return Ok(new
            {
                success = true,
                message = "User updated successfully",
                data = updatedUser
            });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation while updating user {UserId}", id);
            return NotFound(new
            {
                success = false,
                message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user with ID {UserId}", id);
            return StatusCode(500, new
            {
                success = false,
                message = "Error updating user: " + ex.Message
            });
        }
    }

    /// <summary>
    /// Delete user (Admin only) - Soft delete by setting status to INACTIVE
    /// </summary>
    /// <param name="id">User ID</param>
    /// <returns>Deleted user details</returns>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteUser(Guid id)
    {
        try
        {
            var deletedUser = await _authService.DeleteUserAsync(id);

            _logger.LogInformation("User with ID {UserId} has been set to INACTIVE", id);

            return Ok(new
            {
                success = true,
                message = "User deleted successfully (status set to INACTIVE)",
                data = deletedUser
            });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "User with ID {UserId} not found for deletion", id);
            return NotFound(new
            {
                success = false,
                message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user with ID {UserId}", id);
            return StatusCode(500, new
            {
                success = false,
                message = "Error deleting user: " + ex.Message
            });
        }
    }
}