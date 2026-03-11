namespace InventoryService.Application.DTOs;

public class RestockRequestDto
{
    public Guid Id { get; set; }
    public string RequestNumber { get; set; } = string.Empty;
    /// <summary>Source warehouse/store ID (null = external supplier)</summary>
    public Guid? FromWarehouseId { get; set; }
    public string FromLocationType { get; set; } = string.Empty;
    /// <summary>Destination warehouse/store ID (requester's location)</summary>
    public Guid? ToWarehouseId { get; set; }
    public string ToLocationType { get; set; } = string.Empty;
    public Guid RequestedBy { get; set; }
    public DateTime RequestedDate { get; set; }
    public string Priority { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public Guid? ApprovedBy { get; set; }
    public DateTime? ApprovedDate { get; set; }
    public Guid? TransferId { get; set; }
    public string? Notes { get; set; }
    public List<RestockRequestItemDto> Items { get; set; } = new();
}

public class RestockRequestItemDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string? ProductName { get; set; }
    public string? Unit { get; set; }
    public int RequestedQuantity { get; set; }
    public int CurrentQuantity { get; set; }
    public int? ApprovedQuantity { get; set; }
    public string? Reason { get; set; }
}

public class CreateRestockRequestDto
{
    /// <summary>
    /// Source warehouse/store ID.
    /// - Store Manager: ID of the branch warehouse that will send goods
    /// - Warehouse Manager: ID of Kho Tổng (main warehouse)
    /// - Warehouse Admin: null (external supplier)
    /// </summary>
    public Guid? FromWarehouseId { get; set; }
    public string FromLocationType { get; set; } = "WAREHOUSE";

    /// <summary>
    /// Destination warehouse/store ID (requester's own location).
    /// - Store Manager: store ID
    /// - Warehouse Manager: branch warehouse ID
    /// - Warehouse Admin: kho tổng ID
    /// </summary>
    public Guid? ToWarehouseId { get; set; }
    public string ToLocationType { get; set; } = "WAREHOUSE";

    public string Priority { get; set; } = "NORMAL";
    public string? Notes { get; set; }
    public List<CreateRestockRequestItemDto> Items { get; set; } = new();
}

public class CreateRestockRequestItemDto
{
    public Guid ProductId { get; set; }
    public int RequestedQuantity { get; set; }
    public int CurrentQuantity { get; set; }
    public string? Reason { get; set; }
}

/// <summary>
/// Payload for approving a restock request.
/// Approve will create a Transfer and two StockMovements (OUTBOUND from warehouse, INBOUND to store).
/// </summary>
public class ApproveRestockRequestDto
{
    /// <summary>Approved quantity per item, aligned by index with the request's item list.</summary>
    public List<ApproveRestockItemDto> Items { get; set; } = new();

    /// <summary>Expected delivery date for the generated transfer.</summary>
    public DateTime? ExpectedDelivery { get; set; }

    /// <summary>Optional notes for the transfer / approval.</summary>
    public string? Notes { get; set; }
}

public class ApproveRestockItemDto
{
    public Guid RestockItemId { get; set; }
    public int ApprovedQuantity { get; set; }
    public Guid? BatchId { get; set; }  // Which batch to pull from
    public decimal? UnitPrice { get; set; }
}

public class RejectRestockRequestDto
{
    public string Reason { get; set; } = string.Empty;
}

/// <summary>
/// Response returned after approving a restock request.
/// </summary>
public class ApproveRestockResponseDto
{
    public RestockRequestDto RestockRequest { get; set; } = null!;
    public StockMovementDto StockMovement { get; set; } = null!;
}
