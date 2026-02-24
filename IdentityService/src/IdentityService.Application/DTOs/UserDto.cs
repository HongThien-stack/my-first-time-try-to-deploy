namespace IdentityService.Application.DTOs;

public class UserDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? FullName { get; set; }
    public string? Phone { get; set; }
    public string Status { get; set; } = string.Empty;
    public RoleDto Role { get; set; } = null!;
    public bool EmailVerified { get; set; }
}
