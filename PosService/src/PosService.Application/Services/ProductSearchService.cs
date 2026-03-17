using Microsoft.Extensions.Logging;
using PosService.Application.DTOs;
using PosService.Application.Interfaces;

namespace PosService.Application.Services;

/// <summary>
/// Service implementation for product search in POS system
/// Handles business logic and validation for product lookup
/// </summary>
public class ProductSearchService : IProductSearchService
{
    private readonly IProductSearchRepository _productSearchRepository;
    private readonly ILogger<ProductSearchService> _logger;

    // Constants for pagination limits
    private const int DefaultPageSize = 20;
    private const int MaxPageSize = 100;

    public ProductSearchService(
        IProductSearchRepository productSearchRepository,
        ILogger<ProductSearchService> logger)
    {
        _productSearchRepository = productSearchRepository;
        _logger = logger;
    }

    public async Task<ProductSearchResponse> SearchProductsAsync(
        string keyword,
        int pageNumber = 1,
        int pageSize = DefaultPageSize)
    {
        try
        {
            // Validate pagination parameters
            if (pageNumber < 1)
            {
                _logger.LogWarning("Invalid page number: {PageNumber}. Using default: 1", pageNumber);
                pageNumber = 1;
            }

            if (pageSize < 1)
            {
                _logger.LogWarning("Invalid page size: {PageSize}. Using default: {DefaultPageSize}",
                    pageSize, DefaultPageSize);
                pageSize = DefaultPageSize;
            }

            if (pageSize > MaxPageSize)
            {
                _logger.LogWarning("Page size {PageSize} exceeds maximum {MaxPageSize}. Using maximum.",
                    pageSize, MaxPageSize);
                pageSize = MaxPageSize;
            }

            _logger.LogInformation(
                "Searching products - Keyword: '{Keyword}', Page: {PageNumber}, Size: {PageSize}",
                keyword, pageNumber, pageSize);

            // Get search results from repository
            var (items, totalCount) = await _productSearchRepository.SearchProductsAsync(
                keyword,
                pageNumber,
                pageSize);

            // Calculate total pages
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var response = new ProductSearchResponse
            {
                Items = items,
                TotalItems = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalPages = totalPages
            };

            _logger.LogInformation(
                "Search completed - Found {TotalCount} products, returning {ItemCount} items on page {PageNumber}",
                totalCount, items.Count, pageNumber);

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while searching products with keyword: '{Keyword}'", keyword);
            throw;
        }
    }

    public async Task<ProductDetailDto?> GetProductDetailAsync(Guid productId)
    {
        try
        {
            // Validate product ID
            if (productId == Guid.Empty)
            {
                _logger.LogWarning("Invalid product ID: empty GUID");
                return null;
            }

            _logger.LogInformation("Getting product detail for ID: {ProductId}", productId);

            // Get product detail from repository
            var productDetail = await _productSearchRepository.GetProductByIdAsync(productId);

            if (productDetail == null)
            {
                _logger.LogWarning("Product not found or not available: {ProductId}", productId);
                return null;
            }

            _logger.LogInformation(
                "Successfully retrieved product detail: {ProductId} - {ProductName}",
                productId, productDetail.Name);

            return productDetail;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting product detail for ID: {ProductId}", productId);
            throw;
        }
    }
}
