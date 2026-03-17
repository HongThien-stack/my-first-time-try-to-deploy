namespace InventoryService.Application.DTOs;

/// <summary>
/// DTO for creating stock movement from expired batches
/// </summary>
public class CreateOutboundFromExpiredBatchesDto
{
    /// <summary>
    /// Warehouse ID
    /// </summary>
    public Guid WarehouseId { get; set; }

    /// <summary>
    /// Optional notes about the outbound
    /// </summary>
    public string? Notes { get; set; }
}

/// <summary>
/// Response for expired batches stock movement
/// </summary>
public class OutboundStockMovementResponseDto
{
    public Guid StockMovementId { get; set; }
    public string MovementNumber { get; set; } = string.Empty;
    public int TotalBatchesProcessed { get; set; }
    public int TotalQuantityOutbound { get; set; }
    public DateTime MovementDate { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<ExpiredBatchDetailDto> ProcessedBatches { get; set; } = new();
}

/// <summary>
/// Detail of processed expired batch
/// </summary>
public class ExpiredBatchDetailDto
{
    public Guid BatchId { get; set; }
    public string BatchNumber { get; set; } = string.Empty;
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public DateTime? ExpiryDate { get; set; }
}
