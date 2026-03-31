namespace InventoryService.Application.DTOs;

/// <summary>
/// Request to adjust a batch quantity based on physical count and sync inventory.
/// </summary>
public class AdjustBatchInventoryDto
{
    public Guid BatchId { get; set; }
    public int ActualQuantity { get; set; }
    public string? LocationType { get; set; }
    public Guid? LocationId { get; set; }
    public string? Reason { get; set; }
}

/// <summary>
/// Result after adjusting batch and synchronizing inventory.
/// </summary>
public class AdjustBatchInventoryResponseDto
{
    public Guid BatchId { get; set; }
    public Guid ProductId { get; set; }
    public Guid LocationId { get; set; }
    public string LocationType { get; set; } = string.Empty;
    public int OldBatchQuantity { get; set; }
    public int NewBatchQuantity { get; set; }
    public int OldInventoryQuantity { get; set; }
    public int NewInventoryQuantity { get; set; }
    public int InventoryDelta { get; set; }
    public string? MovementNumber { get; set; }
    public string Message { get; set; } = string.Empty;
}