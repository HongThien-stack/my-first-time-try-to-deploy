namespace IdentityService.Application.DTOs;

public class UpdateUserRequestDto
{
    public string? FullName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Password { get; set; }
    public Guid? RoleId { get; set; }
    public bool? IsActive { get; set; }
}
