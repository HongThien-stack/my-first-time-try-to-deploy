namespace InventoryService.Application.DTOs;

/// <summary>
/// DTO for updating a product batch status to expired and creating stock movement
/// </summary>
public class UpdateBatchExpiredDto
{
    /// <summary>
    /// Product batch ID
    /// </summary>
    public Guid BatchId { get; set; }

    /// <summary>
    /// Warehouse ID
    /// </summary>
    public Guid WarehouseId { get; set; }

    /// <summary>
    /// Optional notes about why the batch is expired
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Reason for expiration (e.g., "EXPIRED_DATE" | "QUALITY_ISSUE" | "OTHER")
    /// </summary>
    public string? Reason { get; set; }
}
