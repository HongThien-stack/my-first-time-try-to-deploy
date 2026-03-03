using IdentityService.Application.DTOs;

namespace IdentityService.Application.Interfaces;

public interface IRoleService
{
    Task<RoleDto?> GetRoleByIdAsync(int id);
    Task<IEnumerable<RoleDto>> GetAllRolesAsync();
    Task<RoleDto> CreateRoleAsync(CreateRoleRequest request);
    Task<RoleDto?> UpdateRoleAsync(int id, UpdateRoleRequest request);
}
