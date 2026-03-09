namespace InventoryService.Application.DTOs;

public class InventoryDto
{
    public Guid Id { get; set; }
    public Guid StoreId { get; set; }
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public int AlertThreshold { get; set; }
    public bool IsLowStock => Quantity <= AlertThreshold;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Guid? CreatedBy { get; set; }
    public Guid? UpdatedBy { get; set; }
}

