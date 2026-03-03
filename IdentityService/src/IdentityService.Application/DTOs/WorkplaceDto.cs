namespace IdentityService.Application.DTOs;

/// <summary>
/// Workplace information for users (Manager/Staff)
/// </summary>
public class WorkplaceDto
{
    /// <summary>
    /// Workplace type: WAREHOUSE | STORE
    /// </summary>
    public string Type { get; set; } = string.Empty;
    
    /// <summary>
    /// Workplace ID (references InventoryDB.warehouses.id)
    /// </summary>
    public Guid Id { get; set; }
    
    /// <summary>
    /// Workplace name (optional, can be fetched from InventoryDB)
    /// </summary>
    public string? Name { get; set; }
    
    /// <summary>
    /// Workplace code (optional, can be fetched from InventoryDB)
    /// </summary>
    public string? Code { get; set; }
    
    /// <summary>
    /// Workplace address (optional, can be fetched from InventoryDB)
    /// </summary>
    public string? Address { get; set; }
}
