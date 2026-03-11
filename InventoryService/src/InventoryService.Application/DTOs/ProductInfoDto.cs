namespace InventoryService.Application.DTOs;

/// <summary>
/// Minimal product info fetched from ProductService (inter-service call)
/// </summary>
public class ProductInfoDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public decimal? QuantityPerUnit { get; set; }
}
