namespace InventoryService.Application.Interfaces;

using InventoryService.Application.DTOs;

public interface IProductServiceClient
{
    /// <summary>
    /// Get product info (name, unit) from ProductService by ID.
    /// Returns null if ProductService is unavailable or product not found.
    /// </summary>
    Task<ProductInfoDto?> GetProductByIdAsync(Guid productId);

    /// <summary>
    /// Batch-fetch product info for multiple IDs in parallel.
    /// </summary>
    Task<Dictionary<Guid, ProductInfoDto>> GetProductsByIdsAsync(IEnumerable<Guid> productIds);
}
