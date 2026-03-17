using PosService.Application.DTOs;

namespace PosService.Application.Interfaces;

/// <summary>
/// Service for product search operations in POS system
/// </summary>
public interface IProductSearchService
{
    /// <summary>
    /// Search products for POS by keyword with pagination
    /// </summary>
    /// <param name="keyword">Search term (barcode, SKU, or name)</param>
    /// <param name="pageNumber">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 20, max: 100)</param>
    /// <returns>Paginated product search response</returns>
    Task<ProductSearchResponse> SearchProductsAsync(
        string keyword,
        int pageNumber = 1,
        int pageSize = 20);

    /// <summary>
    /// Get detailed product information by ID for POS checkout
    /// </summary>
    /// <param name="productId">Product unique identifier</param>
    /// <returns>Product detail or null if not found</returns>
    Task<ProductDetailDto?> GetProductDetailAsync(Guid productId);
}
