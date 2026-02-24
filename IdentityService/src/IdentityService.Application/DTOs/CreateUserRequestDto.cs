<<<<<<< HEAD
﻿namespace IdentityService.Application.DTOs;

public class CreateUserRequestDto
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string RoleName { get; set; } = string.Empty; 
    public string? Phone { get; set; }
}
=======
namespace IdentityService.Application.DTOs;

public class CreateUserRequestDto
{
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string RoleName { get; set; } = string.Empty;
}
>>>>>>> 5a39b410e0607bb5d427ecead4ed9085a86bf111
