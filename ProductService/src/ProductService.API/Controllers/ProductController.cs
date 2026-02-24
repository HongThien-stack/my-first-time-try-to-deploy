using Microsoft.AspNetCore.Mvc;
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
    /// PUT /api/products/{id} - Cập nhật sản phẩm
    /// </summary>
    [HttpPut("{id:guid}")]
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
    [HttpDelete("{id:guid}")]
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
