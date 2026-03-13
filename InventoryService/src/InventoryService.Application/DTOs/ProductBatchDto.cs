namespace InventoryService.Application.DTOs;

public class ProductBatchDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public Guid WarehouseId { get; set; }
    public string BatchNumber { get; set; } = string.Empty;
    public int Quantity { get; set; } // Total items in batch
    public DateTime? ManufacturingDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public string? Supplier { get; set; }
    public DateTime ReceivedAt { get; set; }
    public string Status { get; set; } = string.Empty;