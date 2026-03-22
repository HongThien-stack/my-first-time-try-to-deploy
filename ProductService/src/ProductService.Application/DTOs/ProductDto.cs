namespace ProductService.Application.DTOs;

public class ProductDto
{
    public Guid Id { get; set; }
    public string Sku { get; set; } = string.Empty;
    public string? Barcode { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    
    public Guid CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public Guid? SupplierId { get; set; }
    public string? Brand { get; set; }
    public string? Origin { get; set; }
    
    public decimal Price { get; set; }
    public decimal? OriginalPrice { get; set; }
    public string Unit { get; set; } = string.Empty;
    public decimal? Weight { get; set; }
    public decimal? Volume { get; set; }
    
    public bool IsAvailable { get; set; }
    public bool IsFeatured { get; set; }
    public bool IsNew { get; set; }
    public bool IsOnSale { get; set; }
    
    public string? ImageUrl { get; set; }
    public string? Images { get; set; }
    public string? Slug { get; set; }
    
    public DateTime CreatedAt { get; set; }
}
