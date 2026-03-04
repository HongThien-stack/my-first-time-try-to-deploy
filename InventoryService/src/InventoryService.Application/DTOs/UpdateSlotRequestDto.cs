namespace InventoryService.Application.DTOs;

public class UpdateSlotRequestDto
{
    public string? SlotCode { get; set; }
    public string? Zone { get; set; }
    public int? RowNumber { get; set; }
    public int? ColumnNumber { get; set; }
    public string? Status { get; set; } // EMPTY | OCCUPIED | RESERVED | MAINTENANCE
}
