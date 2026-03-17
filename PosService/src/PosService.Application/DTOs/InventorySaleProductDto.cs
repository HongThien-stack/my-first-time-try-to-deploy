namespace PosService.Application.DTOs;

public class InventorySaleProductDto
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? ProductSku { get; set; }
    public int AvailableQuantity { get; set; }
}
