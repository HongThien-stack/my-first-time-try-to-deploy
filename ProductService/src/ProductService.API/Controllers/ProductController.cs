using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductService.API.Helpers;
using ProductService.Application.DTOs;
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
    /// Create a new product
    /// Only Admin, Manager, and Warehouse Staff can create products
    /// </summary>
    /// <param name="request">Product creation request with images</param>
    /// <returns>Created product information</returns>
    [HttpPost]
    [Authorize(Roles = "Admin,Manager,Warehouse Staff")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> CreateProduct([FromForm] CreateProductRequestDto request)
    {
        try
        {
            // Get user information from JWT token
            var userId = JwtHelper.GetUserId(User);
            if (userId == null)
            {
                return Unauthorized(new
                {
                    success = false,
                    message = "User ID không hợp lệ trong token"
                });
            }

            var userName = JwtHelper.GetUserFullName(User) ?? JwtHelper.GetUserEmail(User) ?? "Unknown";
            var userRole = JwtHelper.GetUserRole(User);
            var ipAddress = JwtHelper.GetIpAddress(HttpContext);

            _logger.LogInformation(
                "User {UserName} (ID: {UserId}, Role: {Role}) is creating product {Sku}", 
                userName, userId, userRole, request.Sku);

            // Validate model
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return BadRequest(new
                {
                    success = false,
                    message = "Dữ liệu không hợp lệ",
                    errors = errors
                });
            }

            // Create product
            var result = await _productService.CreateProductAsync(
                request, 
                userId.Value, 
                userName, 
                ipAddress);

            _logger.LogInformation(
                "Product {ProductId} - {ProductName} created successfully by {UserName}", 
                result.Id, result.Name, userName);

            return CreatedAtAction(
                nameof(GetAll), 
                new { id = result.Id }, 
                new
                {
                    success = true,
                    message = "Tạo sản phẩm thành công",
                    data = result
                });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Validation error creating product");
            return BadRequest(new
            {
                success = false,
                message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating product");
            return StatusCode(500, new
            {
                success = false,
                message = "Đã xảy ra lỗi khi tạo sản phẩm. Vui lòng thử lại sau."
            });
        }
    }
}
