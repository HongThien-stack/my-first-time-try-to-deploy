namespace IdentityService.Application.DTOs;

public class UpdateUserRequestDto
{
<<<<<<< HEAD
    public string? Email { get; set; }
    public string? FullName { get; set; }
    public string? Phone { get; set; }
    public string? Password { get; set; }
=======
    public string? FullName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Password { get; set; }
    public Guid? RoleId { get; set; }
    public bool? IsActive { get; set; }
>>>>>>> 5a39b410e0607bb5d427ecead4ed9085a86bf111
}
