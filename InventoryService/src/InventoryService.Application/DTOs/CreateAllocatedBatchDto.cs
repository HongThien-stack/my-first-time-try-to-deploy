namespace InventoryService.Application.DTOs;

/// <summary>
/// Request DTO to allocate/split a product batch from an existing batch
/// </summary>
public class CreateAllocatedBatchDto
{
    /// <summary>
    /// Source batch ID to allocate from
    /// </summary>
    public Guid SourceBatchId { get; set; }

    /// <summary>
    /// Quantity to allocate to the new batch
    /// </summary>
    public int AllocatedQuantity { get; set; }

    /// <summary>
    /// Target warehouse ID for the new batch (if null, same as source)
    /// </summary>
    public Guid? TargetWarehouseId { get; set; }

    /// <summary>
    /// Notes for allocation (optional)
    /// </summary>
    public string? Notes { get; set; }
}
