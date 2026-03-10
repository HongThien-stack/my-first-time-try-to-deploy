namespace InventoryService.Application.DTOs;

public class UpdateTransferStatusDto
{
    /// <summary>
    /// Trạng thái mới: PENDING | IN_TRANSIT | DELIVERED | CANCELLED
    /// </summary>
    public string Status { get; set; } = string.Empty;
    public string? Notes { get; set; }
}