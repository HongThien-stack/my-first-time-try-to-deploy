using Microsoft.AspNetCore.Mvc;
using ProductService.Application.Interfaces;

namespace ProductService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly ILogger<ProductController> _logger;

    public ProductController(IProductService productService, ILogger<ProductController> logger)
    {
        _productService = productService;
        _logger = logger;
    }

    /// <summary>
    /// Get all products
    /// </summary>
    /// <returns>List of all products</returns>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var products = await _productService.GetAllProductsAsync();
            
            _logger.LogInformation("Retrieved {Count} products", products.Count());
            
            return Ok(new
            {
                success = true,
                message = "Products retrieved successfully",
                data = products
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving products");
            return StatusCode(500, new
            {
                success = false,
                message = "An error occurred while retrieving products"
            });
        }
    }

    /// <summary>
    /// Get product details by ID
    /// </summary>
    /// <param name="id">Product ID (GUID)</param>
    /// <returns>Product details or 404 if not found</returns>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        try
        {
            var product = await _productService.GetProductByIdAsync(id);
            
            if (product == null)
            {
                _logger.LogWarning("Product with ID {ProductId} not found", id);
                return NotFound(new
                {
                    success = false,
                    message = "Product not found"
                });
            }
            
            _logger.LogInformation("Retrieved product: {ProductName}", product.Name);
            
            return Ok(new
            {
                success = true,
                message = "Product retrieved successfully",
                data = product
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving product with ID {ProductId}", id);
            return StatusCode(500, new
            {
                success = false,
                message = "An error occurred while retrieving the product"
            });
        }
    }
}
