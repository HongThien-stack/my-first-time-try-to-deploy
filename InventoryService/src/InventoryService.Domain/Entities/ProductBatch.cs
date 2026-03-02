namespace InventoryService.Domain.Entities;

public class ProductBatch
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; } // Reference to ProductDB.products.id
    public Guid WarehouseId { get; set; }
    public string BatchCode { get; set; } = string.Empty;
    public DateTime? ManufactureDate { get; set; }
    public DateTime? ExpirationDate { get; set; }
    public int Quantity { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Guid? CreatedBy { get; set; }
    public Guid? UpdatedBy { get; set; }

    // Navigation property
    public Warehouse? Warehouse { get; set; }
}
