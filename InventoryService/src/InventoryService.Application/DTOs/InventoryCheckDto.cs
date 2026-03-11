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

/// <summary>
/// DTO for creating a new inventory check session
/// Note: CheckedBy will be extracted from authenticated user context
/// </summary>
public class CreateInventoryCheckDto
{
    public string LocationType { get; set; } = string.Empty; // WAREHOUSE | STORE
    public Guid LocationId { get; set; }
    public string CheckType { get; set; } = string.Empty; // FULL | PARTIAL | SPOT
    public string? Notes { get; set; }
}

/// <summary>
/// DTO for submitting inventory check results
/// </summary>
public class SubmitInventoryCheckDto
{
    public List<InventoryCheckItemSubmitDto> Items { get; set; } = new List<InventoryCheckItemSubmitDto>();
}

/// <summary>
/// DTO for individual inventory check item submission
/// </summary>
public class InventoryCheckItemSubmitDto
{
    public Guid ProductId { get; set; }
    public int ActualQuantity { get; set; }
    public string? Note { get; set; }
}

/// <summary>
/// DTO for reconciliation result (discrepancies)
/// </summary>
public class InventoryDiscrepancyDto
{
    public Guid ProductId { get; set; }
    public int SystemQuantity { get; set; }
    public int ActualQuantity { get; set; }
    public int Difference { get; set; }
    public string? Note { get; set; }
}

/// <summary>
/// DTO for approving inventory check
/// Note: ApprovedBy will be extracted from authenticated user context (MANAGER role)
/// </summary>
public class ApproveInventoryCheckDto
{
    public string? Notes { get; set; }
}

/// <summary>
/// DTO for adjusting inventory
/// Note: PerformedBy will be extracted from authenticated user context (MANAGER role)
/// </summary>
public class AdjustInventoryDto
{
    public string? Reason { get; set; }
}
