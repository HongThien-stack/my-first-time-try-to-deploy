using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PosService.Application.DTOs;
using PosService.Application.Interfaces;
using PosService.Infrastructure.Data;

namespace PosService.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for product search optimized for POS
/// Joins ProductDB and InventoryDB for fast lookup during sales
/// </summary>
public class ProductSearchRepository : IProductSearchRepository
{
    private readonly ProductDbContext _productDb;
    private readonly InventoryDbContext _inventoryDb;
    private readonly ILogger<ProductSearchRepository> _logger;

    public ProductSearchRepository(
        ProductDbContext productDb,
        InventoryDbContext inventoryDb,
        ILogger<ProductSearchRepository> logger)
    {
        _productDb = productDb;
        _inventoryDb = inventoryDb;
        _logger = logger;
    }

    public async Task<(List<ProductSearchDto> Items, int TotalCount)> SearchProductsAsync(
        string keyword,
        int pageNumber,
        int pageSize)
    {
        try
        {
            _logger.LogInformation("Searching products with keyword: {Keyword}, Page: {PageNumber}, Size: {PageSize}",
                keyword, pageNumber, pageSize);

            // Normalize keyword for search
            var searchTerm = keyword?.Trim().ToLower() ?? string.Empty;

            // Step 1: Build product query with filters
            var productQuery = _productDb.Products
                .Where(p => p.IsAvailable && !p.IsDeleted)
                .AsQueryable();

            // Step 2: Apply keyword search (barcode, SKU, or name)
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                productQuery = productQuery.Where(p =>
                    (p.Barcode != null && p.Barcode.ToLower().Contains(searchTerm)) ||
                    p.Sku.ToLower().Contains(searchTerm) ||
                    p.Name.ToLower().Contains(searchTerm)
                );
            }

            // Step 3: Get total count before pagination
            var totalCount = await productQuery.CountAsync();

            if (totalCount == 0)
            {
                _logger.LogInformation("No products found for keyword: {Keyword}", keyword);
                return (new List<ProductSearchDto>(), 0);
            }

            // Step 4: Apply sorting
            // Priority: Exact barcode match → Exact SKU match → Name relevance → Alphabetical
            var sortedQuery = productQuery
                .OrderByDescending(p => p.Barcode != null && p.Barcode.ToLower() == searchTerm) // Exact barcode first
                .ThenByDescending(p => p.Sku.ToLower() == searchTerm) // Then exact SKU
                .ThenByDescending(p => p.Name.ToLower().StartsWith(searchTerm)) // Then name starts with
                .ThenBy(p => p.Name); // Finally alphabetical

            // Step 5: Apply pagination
            var products = await sortedQuery
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new
                {
                    p.Id,
                    p.Name,
                    p.Sku,
                    p.Barcode,
                    p.Price,
                    p.ImageUrl,
                    p.Brand,
                    p.Unit,
                    p.IsOnSale
                })
                .ToListAsync();

            // Step 6: Get product IDs for inventory lookup
            var productIds = products.Select(p => p.Id).ToList();

            // Step 7: Get aggregated stock for all products
            var stockData = await _inventoryDb.Inventories
                .Where(i => productIds.Contains(i.ProductId))
                .GroupBy(i => i.ProductId)
                .Select(g => new
                {
                    ProductId = g.Key,
                    AvailableStock = g.Sum(i => i.Quantity - i.ReservedQuantity)
                })
                .ToListAsync();

            // Step 8: Create dictionary for fast lookup
            var stockDict = stockData.ToDictionary(s => s.ProductId, s => s.AvailableStock);

            // Step 9: Map to DTOs with stock information
            var results = products.Select(p => new ProductSearchDto
            {
                ProductId = p.Id,
                Name = p.Name,
                Sku = p.Sku,
                Barcode = p.Barcode,
                Price = p.Price,
                ImageUrl = p.ImageUrl,
                Brand = p.Brand,
                Unit = p.Unit,
                AvailableStock = stockDict.GetValueOrDefault(p.Id, 0),
                IsOnSale = p.IsOnSale
            }).ToList();

            _logger.LogInformation("Found {Count} products for keyword: {Keyword}", results.Count, keyword);

            return (results, totalCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching products with keyword: {Keyword}", keyword);
            throw;
        }
    }

    public async Task<ProductDetailDto?> GetProductByIdAsync(Guid productId)
    {
        try
        {
            _logger.LogInformation("Getting product detail for ID: {ProductId}", productId);

            // Step 1: Query product from ProductDB
            var product = await _productDb.Products
                .Where(p => p.Id == productId && p.IsAvailable && !p.IsDeleted)
                .Select(p => new
                {
                    p.Id,
                    p.Name,
                    p.Sku,
                    p.Barcode,
                    p.Brand,
                    p.Price,
                    p.OriginalPrice,
                    p.Unit,
                    p.ImageUrl,
                    p.IsOnSale
                })
                .FirstOrDefaultAsync();

            if (product == null)
            {
                _logger.LogWarning("Product not found or not available: {ProductId}", productId);
                return null;
            }

            // Step 2: Get aggregated stock from InventoryDB
            var stockData = await _inventoryDb.Inventories
                .Where(i => i.ProductId == productId)
                .Select(i => i.Quantity - i.ReservedQuantity)
                .ToListAsync();

            var availableStock = stockData.Sum();

            // Step 3: Determine stock status based on available quantity
            // Simple logic: OUT_OF_STOCK if 0, LOW_STOCK if <= 10, IN_STOCK otherwise
            string stockStatus;
            if (availableStock == 0)
            {
                stockStatus = "OUT_OF_STOCK";
            }
            else if (availableStock <= 10)
            {
                stockStatus = "LOW_STOCK";
            }
            else
            {
                stockStatus = "IN_STOCK";
            }

            // Step 4: Calculate discount percentage
            decimal? discountPercent = null;
            if (product.OriginalPrice.HasValue && product.OriginalPrice > 0 && product.OriginalPrice > product.Price)
            {
                discountPercent = Math.Round(
                    ((product.OriginalPrice.Value - product.Price) / product.OriginalPrice.Value) * 100,
                    2
                );
            }

            // Step 5: Map to DTO
            var result = new ProductDetailDto
            {
                ProductId = product.Id,
                Name = product.Name,
                Sku = product.Sku,
                Barcode = product.Barcode,
                Description = null, // Not mapped in current DbContext
                Brand = product.Brand,
                Origin = null, // Not mapped in current DbContext
                Price = product.Price,
                OriginalPrice = product.OriginalPrice,
                DiscountPercent = discountPercent,
                Unit = product.Unit,
                Weight = null, // Not mapped in current DbContext
                ImageUrl = product.ImageUrl,
                AvailableStock = availableStock,
                StockStatus = stockStatus,
                IsOnSale = product.IsOnSale
            };

            _logger.LogInformation("Successfully retrieved product detail for: {ProductId}", productId);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting product detail for ID: {ProductId}", productId);
            throw;
        }
    }
}
