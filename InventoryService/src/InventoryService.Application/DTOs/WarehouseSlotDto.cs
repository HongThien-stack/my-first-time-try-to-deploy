namespace InventoryService.Application.DTOs;

public class WarehouseSlotDto
{
    public Guid Id { get; set; }
    public Guid WarehouseId { get; set; }
    public string WarehouseName { get; set; } = string.Empty;
    public string SlotCode { get; set; } = string.Empty;
    public string? Zone { get; set; }
    public int? RowNumber { get; set; }
    public int? ColumnNumber { get; set; }
    public string Status { get; set; } = string.Empty; // EMPTY | OCCUPIED | RESERVED | MAINTENANCE
    public DateTime CreatedAt { get; set; }
}
