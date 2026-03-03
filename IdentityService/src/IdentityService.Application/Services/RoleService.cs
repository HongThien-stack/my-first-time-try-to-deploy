using IdentityService.Application.DTOs;
using IdentityService.Application.Interfaces;
using IdentityService.Domain.Entities;
using IdentityService.Domain.Repositories;

namespace IdentityService.Application.Services;

public class RoleService : IRoleService
{
    private readonly IRoleRepository _roleRepository;

    public RoleService(IRoleRepository roleRepository)
    {
        _roleRepository = roleRepository;
    }

    public async Task<RoleDto?> GetRoleByIdAsync(int id)
    {
        var role = await _roleRepository.GetByIdAsync(id);
        
        if (role == null)
            return null;

        return new RoleDto
        {
            Id = role.Id,
            Name = role.Name,
            Description = role.Description
        };
    }

    public async Task<IEnumerable<RoleDto>> GetAllRolesAsync()
    {
        var roles = await _roleRepository.GetAllAsync();
        
        return roles.Select(r => new RoleDto
        {
            Id = r.Id,
            Name = r.Name,
            Description = r.Description
        });
    }

    public async Task<RoleDto> CreateRoleAsync(CreateRoleRequest request)
    {
        // Kiểm tra xem role đã tồn tại chưa
        var existingRole = await _roleRepository.GetByNameAsync(request.Name);
        if (existingRole != null)
        {
            throw new InvalidOperationException($"Role with name '{request.Name}' already exists");
        }

        var role = new Role
        {
            Name = request.Name,
            Description = request.Description,
            IsSystem = false,
            CreatedAt = DateTime.UtcNow
        };

        var createdRole = await _roleRepository.CreateAsync(role);

        return new RoleDto
        {
            Id = createdRole.Id,
            Name = createdRole.Name,
            Description = createdRole.Description
        };
    }

    public async Task<RoleDto?> UpdateRoleAsync(int id, UpdateRoleRequest request)
    {
        var existingRole = await _roleRepository.GetByIdAsync(id);
        if (existingRole == null)
            return null;

        // Kiểm tra xem không cho sửa role system
        if (existingRole.IsSystem)
        {
            throw new InvalidOperationException("Cannot update system roles");
        }

        // Kiểm tra xem tên mới đã bị trùng với role khác chưa
        if (existingRole.Name != request.Name)
        {
            var roleWithSameName = await _roleRepository.GetByNameAsync(request.Name);
            if (roleWithSameName != null)
            {
                throw new InvalidOperationException($"Role with name '{request.Name}' already exists");
            }
        }

        existingRole.Name = request.Name;
        existingRole.Description = request.Description;

        var updatedRole = await _roleRepository.UpdateAsync(existingRole);
        if (updatedRole == null)
            return null;

        return new RoleDto
        {
            Id = updatedRole.Id,
            Name = updatedRole.Name,
            Description = updatedRole.Description
        };
    }
}
