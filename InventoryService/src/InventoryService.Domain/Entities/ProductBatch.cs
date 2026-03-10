namespace InventoryService.Domain.Entities;

public class ProductBatch
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; } // ProductDB.products.id
    public Guid WarehouseId { get; set; }
    public Guid? SlotId { get; set; } // warehouse_slots.id
    public string BatchNumber { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public DateTime? ManufacturingDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public string? Supplier { get; set; }
    public Guid? SupplierId { get; set; }       // ProductDB.suppliers.id (logic reference)
    public Guid? PurchaseOrderId { get; set; }  // purchase_orders.id
    public DateTime ReceivedAt { get; set; }
    public string Status { get; set; } = "AVAILABLE"; // AVAILABLE | SOLD | EXPIRED | DAMAGED

    // Navigation properties
    public Warehouse Warehouse { get; set; } = null!;
    public WarehouseSlot? WarehouseSlot { get; set; }
    public ICollection<TransferItem> TransferItems { get; set; } = new List<TransferItem>();
    public ICollection<StockMovementItem> StockMovementItems { get; set; } = new List<StockMovementItem>();
}
