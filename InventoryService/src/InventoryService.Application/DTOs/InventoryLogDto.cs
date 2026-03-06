namespace InventoryService.Application.DTOs;

public class InventoryLogDto
{
    public long Id { get; set; }
    public Guid InventoryId { get; set; }
    public Guid ProductId { get; set; }
    public string Action { get; set; } = string.Empty; // ADJUST | RECEIVE | TRANSFER | SALE | DAMAGE
    public int OldQuantity { get; set; }
    public int NewQuantity { get; set; }
    public int QuantityChange { get; set; }
    public string? Reason { get; set; }
    public Guid PerformedBy { get; set; }
    public DateTime PerformedAt { get; set; }
}
