namespace PosService.Application.DTOs;

/// <summary>
/// DTO representing a product in search results for POS system
/// </summary>
public class ProductSearchDto
{
    public Guid ProductId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public string? Barcode { get; set; }
    public decimal Price { get; set; }
    public string? ImageUrl { get; set; }
    public string? Brand { get; set; }
    public string Unit { get; set; } = string.Empty;
    public int AvailableStock { get; set; }
    public bool IsOnSale { get; set; }
}
