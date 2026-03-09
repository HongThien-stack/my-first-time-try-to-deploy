namespace InventoryService.Application.DTOs;

public class CreateSlotRequestDto
{
    public string SlotCode { get; set; } = string.Empty; // e.g. A-01-01
    public string? Zone { get; set; }
    public int? RowNumber { get; set; }
    public int? ColumnNumber { get; set; }
    public string Status { get; set; } = "EMPTY"; // EMPTY | OCCUPIED | RESERVED | MAINTENANCE
}
