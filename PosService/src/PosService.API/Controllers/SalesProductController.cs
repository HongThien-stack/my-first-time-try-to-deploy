using Microsoft.AspNetCore.Mvc;
using PosService.Application.Interfaces;

namespace PosService.API.Controllers;

[ApiController]
[Route("api/sales/products")]
public class SalesProductController : ControllerBase
{
    private readonly IProductSearchService _productSearchService;
    private readonly ILogger<SalesProductController> _logger;

    public SalesProductController(
        IProductSearchService productSearchService,
        ILogger<SalesProductController> logger)
    {
        _productSearchService = productSearchService;
        _logger = logger;
    }

    /// <summary>
    /// Search products for POS system by barcode, SKU, or name
    /// Optimized for fast lookup during sales transactions
    /// </summary>
    /// <param name="keyword">Search keyword (barcode, SKU, or product name)</param>
    /// <param name="pageNumber">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 20, max: 100)</param>
    /// <returns>Paginated list of products with stock information</returns>
    /// <response code="200">Returns paginated product search results</response>
    /// <response code="400">If search parameters are invalid</response>
    /// <response code="500">If an error occurs during search</response>
    [HttpGet("search")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> SearchProducts(
        [FromQuery] string? keyword,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20)
    {
        try
        {
            // Validate page parameters
            if (pageNumber < 1)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Page number must be greater than 0"
                });
            }

            if (pageSize < 1 || pageSize > 100)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Page size must be between 1 and 100"
                });
            }

            _logger.LogInformation(
                "Product search request - Keyword: '{Keyword}', Page: {PageNumber}, Size: {PageSize}",
                keyword, pageNumber, pageSize);

            // Perform search
            var result = await _productSearchService.SearchProductsAsync(
                keyword ?? string.Empty,
                pageNumber,
                pageSize);

            return Ok(new
            {
                success = true,
                message = $"Found {result.TotalItems} products matching '{keyword}'",
                data = result
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching products with keyword: '{Keyword}'", keyword);
            return StatusCode(500, new
            {
                success = false,
                message = "An error occurred while searching products",
                error = ex.Message
            });
        }
    }

    /// <summary>
    /// Search product by exact barcode (optimized for barcode scanner)
    /// Returns single product if found, null if not found
    /// </summary>
    /// <param name="barcode">Product barcode</param>
    /// <returns>Single product or not found</returns>
    [HttpGet("search/barcode/{barcode}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> SearchByBarcode(string barcode)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(barcode))
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Barcode cannot be empty"
                });
            }

            _logger.LogInformation("Barcode search - Barcode: {Barcode}", barcode);

            // Search with exact barcode (page size 1 since we expect single result)
            var result = await _productSearchService.SearchProductsAsync(barcode, 1, 1);

            if (result.Items.Count == 0)
            {
                return NotFound(new
                {
                    success = false,
                    message = $"Product with barcode '{barcode}' not found"
                });
            }

            var product = result.Items.First();

            // Verify exact barcode match (since search is partial match)
            if (!string.Equals(product.Barcode, barcode, StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(new
                {
                    success = false,
                    message = $"Product with barcode '{barcode}' not found"
                });
            }

            return Ok(new
            {
                success = true,
                message = "Product found",
                data = product
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching by barcode: {Barcode}", barcode);
            return StatusCode(500, new
            {
                success = false,
                message = "An error occurred while searching by barcode",
                error = ex.Message
            });
        }
    }

    /// <summary>
    /// Get detailed product information by ID for POS checkout flow
    /// Returns complete product details including pricing, stock status, and attributes
    /// </summary>
    /// <param name="id">Product unique identifier</param>
    /// <returns>Detailed product information</returns>
    /// <response code="200">Returns product detail</response>
    /// <response code="404">If product not found or not available</response>
    /// <response code="500">If an error occurs</response>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetProductDetail(Guid id)
    {
        try
        {
            _logger.LogInformation("Getting product detail for ID: {ProductId}", id);

            // Validate product ID
            if (id == Guid.Empty)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Invalid product ID"
                });
            }

            // Get product detail
            var productDetail = await _productSearchService.GetProductDetailAsync(id);

            if (productDetail == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = $"Product with ID '{id}' not found or not available for sale"
                });
            }

            return Ok(new
            {
                success = true,
                message = "Product detail retrieved successfully",
                data = productDetail
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting product detail for ID: {ProductId}", id);
            return StatusCode(500, new
            {
                success = false,
                message = "An error occurred while retrieving product detail",
                error = ex.Message
            });
        }
    }
}
