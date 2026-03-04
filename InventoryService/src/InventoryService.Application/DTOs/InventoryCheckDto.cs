namespace InventoryService.Application.DTOs;

/// <summary>
/// DTO for inventory check list view
/// </summary>
public class InventoryCheckListDto
{
    public Guid Id { get; set; }
    public string CheckNumber { get; set; } = string.Empty;
    public string LocationType { get; set; } = string.Empty;
    public Guid LocationId { get; set; }
    public string CheckType { get; set; } = string.Empty;
    public DateTime CheckDate { get; set; }
    public Guid CheckedBy { get; set; }
    public string Status { get; set; } = string.Empty;
    public int TotalDiscrepancies { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// DTO for inventory check detail view
/// </summary>
public class InventoryCheckDto
{
    public Guid Id { get; set; }
    public string CheckNumber { get; set; } = string.Empty;
    public string LocationType { get; set; } = string.Empty;
    public Guid LocationId { get; set; }
    public string CheckType { get; set; } = string.Empty;
    public DateTime CheckDate { get; set; }
    public Guid CheckedBy { get; set; }
    public string Status { get; set; } = string.Empty;
    public int TotalDiscrepancies { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<InventoryCheckItemDto> Items { get; set; } = new List<InventoryCheckItemDto>();
}

/// <summary>
/// DTO for inventory check item
/// </summary>
public class InventoryCheckItemDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public int SystemQuantity { get; set; }
    public int ActualQuantity { get; set; }
    public int Difference { get; set; }
    public string? Note { get; set; }
}
