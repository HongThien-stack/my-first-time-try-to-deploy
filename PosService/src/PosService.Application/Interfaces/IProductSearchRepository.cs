using PosService.Application.DTOs;

namespace PosService.Application.Interfaces;

/// <summary>
/// Repository for searching products in POS context
/// Combines data from ProductDB and InventoryDB
/// </summary>
public interface IProductSearchRepository
{
    /// <summary>
    /// Search products by keyword (barcode, SKU, or name) with stock information
    /// </summary>
    /// <param name="keyword">Search term (barcode, SKU, or product name)</param>
    /// <param name="pageNumber">Page number (1-based)</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <returns>Paginated search results with stock information</returns>
    Task<(List<ProductSearchDto> Items, int TotalCount)> SearchProductsAsync(
        string keyword,
        int pageNumber,
        int pageSize);

    /// <summary>
    /// Get detailed product information by ID with stock availability
    /// </summary>
    /// <param name="productId">Product unique identifier</param>
    /// <returns>Product detail or null if not found/not available</returns>
    Task<ProductDetailDto?> GetProductByIdAsync(Guid productId);
}
