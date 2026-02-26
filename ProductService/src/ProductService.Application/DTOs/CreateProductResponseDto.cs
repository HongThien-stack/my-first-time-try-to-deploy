namespace ProductService.Application.DTOs;

public class CreateProductResponseDto
{
    public Guid Id { get; set; }
    public string Sku { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Barcode { get; set; }
    public Guid CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public decimal Price { get; set; }
    public string Unit { get; set; } = string.Empty;
    public string? Brand { get; set; }
    public string? Origin { get; set; }
    public string? ImageUrl { get; set; }
    public List<string>? Images { get; set; }
    public string? Slug { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid? CreatedBy { get; set; }
    public string? CreatedByName { get; set; }
}
