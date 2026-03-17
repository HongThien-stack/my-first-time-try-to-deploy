namespace InventoryService.Application.DTOs;

/// <summary>
/// Response DTO for batch expiration operation
/// </summary>
public class UpdateBatchExpiredResponseDto
{
    /// <summary>
    /// The batch ID that was expired
    /// </summary>
    public Guid BatchId { get; set; }

    /// <summary>
    /// Batch number for reference
    /// </summary>
    public string BatchNumber { get; set; } = string.Empty;

    /// <summary>
    /// Product ID
    /// </summary>
    public Guid ProductId { get; set; }

    /// <summary>
    /// Quantity in the expired batch
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// New status of the batch (EXPIRED)
    /// </summary>
    public string NewStatus { get; set; } = "EXPIRED";

    /// <summary>
    /// Stock movement ID created for the outbound
    /// </summary>
    public Guid StockMovementId { get; set; }

    /// <summary>
    /// Movement number for reference
    /// </summary>
    public string MovementNumber { get; set; } = string.Empty;

    /// <summary>
    /// Date of the movement
    /// </summary>
    public DateTime MovementDate { get; set; }

    /// <summary>
    /// Success message
    /// </summary>
    public string Message { get; set; } = "Batch marked as expired and stock movement outbound created";
}
