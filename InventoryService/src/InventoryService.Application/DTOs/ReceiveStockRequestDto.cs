namespace InventoryService.Application.DTOs;

/// <summary>
/// DTO cho API nhập hàng từ Supplier vào Warehouse
/// </summary>
public class ReceiveStockRequestDto
{
    public Guid WarehouseId { get; set; }
    public string Supplier { get; set; } = string.Empty;
    public Guid? PurchaseOrderId { get; set; }
    public Guid? TransferId { get; set; }
    public string? Notes { get; set; }
    public List<ReceiveStockItemDto> Items { get; set; } = [];
}

public class ReceiveStockItemDto
{
    public Guid ProductId { get; set; }
    public Guid? SlotId { get; set; }
    public int Quantity { get; set; }
    public decimal? UnitPrice { get; set; }
    public string? BatchNumber { get; set; }
    public DateTime? ManufacturingDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
}