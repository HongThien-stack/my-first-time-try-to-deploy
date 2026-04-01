namespace IdentityService.Application.DTOs;

public class UpdateUserRequestDto
{
    public string? Email { get; set; }
    public string? FullName { get; set; }
    public int? RoleId { get; set; }
    
    /// <summary>
    /// Workplace type: WAREHOUSE | STORE | NULL
    /// </summary>
    public string? WorkplaceType { get; set; }
    
    /// <summary>
    /// Workplace ID (references InventoryDB.warehouses.id)
    /// </summary>
    public Guid? WorkplaceId { get; set; }
}
