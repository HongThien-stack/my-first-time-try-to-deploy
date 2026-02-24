namespace IdentityService.Application.DTOs;

public class UpdateUserRequestDto
{
    public string? Email { get; set; }
    public string? FullName { get; set; }
    public string? Phone { get; set; }
    public string? Password { get; set; }
}
