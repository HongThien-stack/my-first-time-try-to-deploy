namespace IdentityService.Application.DTOs;

public class LoginResponseDto
{
    public string AccessToken { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? FullName { get; set; }
    public string? Phone { get; set; }
    public int RoleId { get; set; }
    public bool IsEmailVerified { get; set; }
    public string? Message { get; set; }
    
    /// <summary>
    /// Workplace information (included in login response for Manager/Staff)
    /// </summary>
    public WorkplaceDto? Workplace { get; set; }
}
