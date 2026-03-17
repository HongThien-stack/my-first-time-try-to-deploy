namespace PosService.Application.DTOs;

/// <summary>
/// Detailed product information for POS checkout flow
/// </summary>
public class ProductDetailDto
{
    /// <summary>
    /// Product unique identifier
    /// </summary>
    public Guid ProductId { get; set; }

    /// <summary>
    /// Product name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Product SKU code
    /// </summary>
    public string Sku { get; set; } = string.Empty;

    /// <summary>
    /// Product barcode
    /// </summary>
    public string? Barcode { get; set; }

    /// <summary>
    /// Product description
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Brand name
    /// </summary>
    public string? Brand { get; set; }

    /// <summary>
    /// Country/region of origin
    /// </summary>
    public string? Origin { get; set; }

    /// <summary>
    /// Current selling price
    /// </summary>
    public decimal Price { get; set; }

    /// <summary>
    /// Original price before discount
    /// </summary>
    public decimal? OriginalPrice { get; set; }

    /// <summary>
    /// Discount percentage (calculated)
    /// </summary>
    public decimal? DiscountPercent { get; set; }

    /// <summary>
    /// Unit of measure (Hộp, Chai, Kg, etc.)
    /// </summary>
    public string Unit { get; set; } = string.Empty;

    /// <summary>
    /// Product weight in kg
    /// </summary>
    public decimal? Weight { get; set; }

    /// <summary>
    /// Product image URL
    /// </summary>
    public string? ImageUrl { get; set; }

    /// <summary>
    /// Total available stock across all locations
    /// </summary>
    public decimal AvailableStock { get; set; }

    /// <summary>
    /// Stock status: IN_STOCK, LOW_STOCK, OUT_OF_STOCK
    /// </summary>
    public string StockStatus { get; set; } = string.Empty;

    /// <summary>
    /// Whether product is currently on sale
    /// </summary>
    public bool IsOnSale { get; set; }
}
