using Microsoft.Extensions.Logging;
using PosService.Application.DTOs;
using PosService.Application.Interfaces;
using PosService.Application.Interfaces.Http;

namespace PosService.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for product search optimized for POS
/// Calls ProductService and InventoryService via HTTP to get data
/// </summary>
public class ProductSearchRepository : IProductSearchRepository
{
    private readonly IProductServiceClient _productServiceClient;
    private readonly IInventoryServiceClient _inventoryServiceClient;
    private readonly ILogger<ProductSearchRepository> _logger;

    public ProductSearchRepository(
        IProductServiceClient productServiceClient,
        IInventoryServiceClient inventoryServiceClient,
        ILogger<ProductSearchRepository> logger)
    {
        _productServiceClient = productServiceClient;
        _inventoryServiceClient = inventoryServiceClient;
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

            // Step 1: Call ProductService to get product data
            var searchResult = await _productServiceClient.SearchProductsAsync(keyword, pageNumber, pageSize);
            if (searchResult == null || !searchResult.Value.Items.Any())
            {
                _logger.LogInformation("No products found from ProductService for keyword: {Keyword}", keyword);
                return (new List<ProductSearchDto>(), 0);
            }

            var products = searchResult.Value.Items;
            var totalCount = searchResult.Value.TotalCount;

            // Step 2: Get product IDs for inventory lookup
            var productIds = products.Select(p => p.Id).ToList();

            // Step 3: Call InventoryService to get stock levels
            var stockData = await _inventoryServiceClient.GetStockLevelsBatchAsync(productIds);

            // Step 4: Combine product data with stock data
            var resultItems = products.Select(p => new ProductSearchDto
            {
                ProductId = p.Id,
                Name = p.Name,
                Sku = p.Sku,
                Barcode = p.Barcode,
                Price = p.Price,
                ImageUrl = p.ImageUrl,
                Brand = p.Brand,
                Unit = p.Unit,
                IsOnSale = p.IsOnSale,
                AvailableStock = stockData.TryGetValue(p.Id, out var stock) ? stock : 0
            }).ToList();

            return (resultItems, totalCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred during product search.");
            return (new List<ProductSearchDto>(), 0);
        }
    }

    // This method is kept for compatibility but should be deprecated or refactored
    // as it still implies direct DB access which we are moving away from.
    // For now, it will return a simplified object or throw NotImplementedException.
    public Task<ProductDetailDto?> GetProductByIdAsync(Guid productId)
    {
        return GetProductByIdInternalAsync(productId);
    }

    private async Task<ProductDetailDto?> GetProductByIdInternalAsync(Guid productId)
    {
        try
        {
            var products = await _productServiceClient.GetProductDetailsBatchAsync(new[] { productId });
            var product = products.FirstOrDefault();

            if (product == null)
            {
                return null;
            }

            var stockMap = await _inventoryServiceClient.GetStockLevelsBatchAsync(new[] { productId });
            var availableStock = stockMap.TryGetValue(productId, out var stock) ? stock : 0;

            return new ProductDetailDto
            {
                ProductId = product.Id,
                Name = product.Name,
                Sku = product.Sku,
                Barcode = product.Barcode,
                Description = null,
                Brand = product.Brand,
                Origin = null,
                Price = product.Price,
                OriginalPrice = null,
                DiscountPercent = null,
                Unit = product.Unit,
                Weight = null,
                ImageUrl = product.ImageUrl,
                AvailableStock = availableStock,
                StockStatus = availableStock <= 0 ? "OUT_OF_STOCK" : availableStock <= 10 ? "LOW_STOCK" : "IN_STOCK",
                IsOnSale = product.IsOnSale
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get product detail for ID: {ProductId}", productId);
            return null;
        }
    }
}

