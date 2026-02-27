using Microsoft.AspNetCore.Mvc;
using ProductService.Application.DTOs;
using ProductService.Application.Interfaces;
using Productservice.Application.Services;
namespace ProductService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoryController : ControllerBase
{
    private readonly ICategoryService _categoryService;
    private readonly ILogger<CategoryController> _logger;

    public CategoryController(ICategoryService categoryService, ILogger<CategoryController> logger)
    {
        _categoryService = categoryService;
        _logger = logger;
    }

    // get categories by id 
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var category = await _categoryService.GetCategoryByIdAsync(id);

        if (category is null)
            return NotFound(new { success = false, message = $"Category '{id}' not found" });

        return Ok(new { success = true, message = "Category retrieved successfully", data = category });
    }

    // update category by id
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCategoryRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(new
            {
                success = false,
                message = "Validation failed",
                errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)
            });

        var updated = await _categoryService.UpdateCategoryAsync(id, request);

        if (updated is null)
            return NotFound(new { success = false, message = $"Category '{id}' not found" });

        return Ok(new { success = true, message = "Category updated successfully", data = updated });
    }

    
    /// <summary>
    /// Delete category by ID
    /// Cannot delete if category has products
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            var deleted = await _categoryService.DeleteCategoryAsync(id);

            if (!deleted)
                return NotFound(new { success = false, message = $"Category '{id}' not found" });

            return Ok(new { success = true, message = "Category deleted successfully" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting category {CategoryId}", id);
            return StatusCode(500, new { success = false, message = "Đã xảy ra lỗi khi xóa danh mục" });
        }
    }

}