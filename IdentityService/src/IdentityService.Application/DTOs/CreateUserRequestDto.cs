namespace IdentityService.Application.DTOs;

public class CreateUserRequestDto
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string RoleName { get; set; } = string.Empty; 
    public string? Phone { get; set; }
    
    /// <summary>
    /// Workplace type: WAREHOUSE | STORE | NULL (for Admin/Customer)
    /// </summary>
    public string? WorkplaceType { get; set; }
    
    /// <summary>
    /// Workplace ID (references InventoryDB.warehouses.id)
    /// </summary>
    public Guid? WorkplaceId { get; set; }
}