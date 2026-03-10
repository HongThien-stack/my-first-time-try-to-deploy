namespace InventoryService.Application.DTOs;

public class StockMovementDto
{
    public Guid Id { get; set; }
    public string MovementNumber { get; set; } = string.Empty;
    public string MovementType { get; set; } = string.Empty; // INBOUND | OUTBOUND | TRANSFER | ADJUSTMENT
    public Guid LocationId { get; set; }
    public string LocationType { get; set; } = string.Empty; // WAREHOUSE | STORE
    public DateTime MovementDate { get; set; }
    public string? Supplier { get; set; }
    public Guid? PurchaseOrderId { get; set; }
    public Guid? TransferId { get; set; }
    public Guid? ReceivedBy { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public int TotalItems { get; set; }
    public IEnumerable<StockMovementItemDto> Items { get; set; } = [];
}