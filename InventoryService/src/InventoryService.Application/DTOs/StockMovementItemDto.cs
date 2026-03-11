namespace InventoryService.Application.DTOs;

public class StockMovementItemDto
{
    public Guid Id { get; set; }
    public Guid MovementId { get; set; }
    public Guid ProductId { get; set; }
    public string? ProductName { get; set; }
    public string? Unit { get; set; }
    public Guid? BatchId { get; set; }
    public int Quantity { get; set; }
    public decimal? UnitPrice { get; set; }
}