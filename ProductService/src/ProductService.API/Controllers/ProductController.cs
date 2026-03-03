using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductService.API.Helpers;
using ProductService.Application.DTOs;
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
    /// GET /api/products - Lấy danh sách tất cả sản phẩm
    /// </summary>
    [HttpGet("Get-All-Products")]
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
    [HttpPost("Add-Product")]
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
                userId.Value);

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
            
            // Return detailed error in development
            var isDevelopment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development";
            
            return StatusCode(500, new
            {
                success = false,
                message = "Đã xảy ra lỗi khi tạo sản phẩm. Vui lòng thử lại sau.",
                error = isDevelopment ? ex.Message : null,
                stackTrace = isDevelopment ? ex.StackTrace : null,
                innerError = isDevelopment && ex.InnerException != null ? ex.InnerException.Message : null
            });
        }
    }

    /// <summary>
    /// Get product details by ID
    /// </summary>
    /// <param name="id">Product ID (GUID)</param>
    /// <returns>Product details or 404 if not found</returns>
    [HttpGet("Get-Product-by-ID")]
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

    /// <summary>
    /// PUT /api/products/{id} - Cập nhật sản phẩm
    /// </summary>
    [HttpPut("Update-Product")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProductRequestDto? request)
    {
        try
        {
            if (request == null)
                return BadRequest(new { success = false, message = "Body request không được để trống" });

            if (!ModelState.IsValid)
                return BadRequest(new
                {
                    success = false,
                    message = "Dữ liệu gửi lên không hợp lệ",
                    errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                });

            var updated = await _productService.UpdateProductAsync(id, request);

            if (updated == null)
                return NotFound(new
                {
                    success = false,
                    message = $"Không tìm thấy sản phẩm với id '{id}'"
                });

            _logger.LogInformation("Product {ProductId} updated successfully", id);

            return Ok(new
            {
                success = true,
                message = "Cập nhật sản phẩm thành công",
                data = updated
            });
        }
        catch (ArgumentException ex)
        {
            // Lỗi nghiệp vụ — trả 400 với mô tả rõ ràng
            _logger.LogWarning("Business rule violation for product {ProductId}: {Message}", id, ex.Message);
            return BadRequest(new
            {
                success = false,
                message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating product {ProductId}", id);
            return StatusCode(500, new
            {
                success = false,
                message = "Lỗi khi cập nhật sản phẩm"
            });
        }
    }

    /// <summary>
    /// DELETE /api/products/{id} - Xóa mềm sản phẩm (soft delete: set is_available = false)
    /// </summary>
    [HttpDelete("Delete-Product")]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            var result = await _productService.DeleteProductAsync(id);

            if (!result)
                return NotFound(new
                {
                    success = false,
                    message = $"Không tìm thấy sản phẩm với id '{id}'"
                });

            _logger.LogInformation("Product {ProductId} soft-deleted", id);

            return Ok(new
            {
                success = true,
                message = "Xóa sản phẩm thành công"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting product {ProductId}", id);
            return StatusCode(500, new
            {
                success = false,
                message = "Lỗi khi xóa sản phẩm"
            });
        }
    }
}
