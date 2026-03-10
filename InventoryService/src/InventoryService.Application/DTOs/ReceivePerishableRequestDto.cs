namespace InventoryService.Application.DTOs;

/// <summary>
/// DTO cho API nhập hàng tươi sống thẳng vào Store (bỏ qua Warehouse)
/// </summary>
public class ReceivePerishableRequestDto
{
    public Guid StoreId { get; set; }
    public string Supplier { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public List<ReceivePerishableItemDto> Items { get; set; } = [];
}

public class ReceivePerishableItemDto
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal? UnitPrice { get; set; }
    public string? BatchNumber { get; set; }
    public DateTime ManufacturingDate { get; set; }
    public DateTime ExpiryDate { get; set; }
}