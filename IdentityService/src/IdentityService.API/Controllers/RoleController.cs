using IdentityService.Application.DTOs;
using IdentityService.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IdentityService.API.Controllers;

[ApiController]
[Route("api/roles")]
[Tags("Role Management")]
[Authorize(Roles = "Admin")]
public class RoleController : ControllerBase
{
    private readonly IRoleService _roleService;
    private readonly ILogger<RoleController> _logger;

    public RoleController(IRoleService roleService, ILogger<RoleController> logger)
    {
        _roleService = roleService;
        _logger = logger;
    }

    /// <summary>
    /// Get role details by ID (Admin only)
    /// </summary>
    /// <param name="id">Role ID</param>
    /// <returns>Role details</returns>
    [HttpGet("Get-Role-By-Id")]
    public async Task<IActionResult> GetRoleById(int id)
    {
        try
        {
            var role = await _roleService.GetRoleByIdAsync(id);

            if (role == null)
            {
                _logger.LogWarning("Role with ID {RoleId} not found", id);
                return NotFound(new
                {
                    success = false,
                    message = $"Role with ID {id} not found"
                });
            }

            _logger.LogInformation("Retrieved role {RoleName} (ID: {RoleId})", role.Name, role.Id);

            return Ok(new
            {
                success = true,
                message = "Role retrieved successfully",
                data = role
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving role with ID {RoleId}", id);
            return StatusCode(500, new
            {
                success = false,
                message = "Error retrieving role: " + ex.Message
            });
        }
    }

    /// <summary>
    /// Get all roles (Admin only)
    /// </summary>
    /// <returns>List of all roles</returns>
    [HttpGet("Get-All-Roles")]
    public async Task<IActionResult> GetAllRoles()
    {
        try
        {
            var roles = await _roleService.GetAllRolesAsync();

            _logger.LogInformation("Retrieved {Count} roles", roles.Count());

            return Ok(new
            {
                success = true,
                message = "Roles retrieved successfully",
                data = roles
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all roles");
            return StatusCode(500, new
            {
                success = false,
                message = "Error retrieving roles: " + ex.Message
            });
        }
    }

    /// <summary>
    /// Create a new role (Admin only)
    /// </summary>
    /// <param name="request">Role details</param>
    /// <returns>Created role</returns>
    [HttpPost("Create-Role")]
    public async Task<IActionResult> CreateRole([FromBody] CreateRoleRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Validation failed",
                    errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)
                });
            }

            var role = await _roleService.CreateRoleAsync(request);

            _logger.LogInformation("Created new role {RoleName} (ID: {RoleId})", role.Name, role.Id);

            return CreatedAtAction(
                nameof(GetRoleById),
                new { id = role.Id },
                new
                {
                    success = true,
                    message = "Role created successfully",
                    data = role
                });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Failed to create role: {Message}", ex.Message);
            return BadRequest(new
            {
                success = false,
                message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating role");
            return StatusCode(500, new
            {
                success = false,
                message = "Error creating role: " + ex.Message
            });
        }
    }

    /// <summary>
    /// Update an existing role (Admin only)
    /// </summary>
    /// <param name="id">Role ID</param>
    /// <param name="request">Updated role details</param>
    /// <returns>Updated role</returns>
    [HttpPut("Update-Role")]
    public async Task<IActionResult> UpdateRole(int id, [FromBody] UpdateRoleRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Validation failed",
                    errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)
                });
            }

            var role = await _roleService.UpdateRoleAsync(id, request);

            if (role == null)
            {
                _logger.LogWarning("Role with ID {RoleId} not found for update", id);
                return NotFound(new
                {
                    success = false,
                    message = $"Role with ID {id} not found"
                });
            }

            _logger.LogInformation("Updated role {RoleName} (ID: {RoleId})", role.Name, role.Id);

            return Ok(new
            {
                success = true,
                message = "Role updated successfully",
                data = role
            });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Failed to update role: {Message}", ex.Message);
            return BadRequest(new
            {
                success = false,
                message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating role with ID {RoleId}", id);
            return StatusCode(500, new
            {
                success = false,
                message = "Error updating role: " + ex.Message
            });
        }
    }
}
