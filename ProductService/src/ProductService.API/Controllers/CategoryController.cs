using Microsoft.AspNetCore.Mvc;
using ProductService.Application.DTOs;
using ProductService.Application.Interfaces;
using ProductService.Domain.Entities;

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

    [HttpGet("get-all-categories")]
    public async Task<ActionResult<List<CategoryResponse>>> GetAllCategories()
    {
        List<CategoryResponse> categoryResponses = new List<CategoryResponse>();
        var categories = await _categoryService.GetAllCategories();
        if (categories == null || categories.Count == 0)
        {
            return NotFound("No categories found.");
        }

        foreach (var category in categories)
        {
            categoryResponses.Add(new CategoryResponse
            {
                Name = category.Name,
                Status = category.Status,
                CreatedAt = category.CreatedAt,
                UpdatedAt = category.UpdatedAt
            });
        }
        return Ok(categoryResponses);
    }

    [HttpPost("add-category")]
    public ActionResult AddCategory([FromBody] CategoryRequest categoryRequest)
    {
        string name = categoryRequest.Name;
        if (string.IsNullOrEmpty(name))
        {
            return BadRequest("Name is required.");
        }
        Guid id = Guid.NewGuid();
        string status = "ACTIVE";
        DateTime createdAt = DateTime.UtcNow;
        DateTime? updatedAt = null;
        Category category = new Category
        {
            Id = id,
            Name = name,
            Status = status,
            CreatedAt = createdAt,
            UpdatedAt = updatedAt
        };
        _categoryService.AddCategory(category);
        return Ok("Category added successfully.");
    }

    // get categories by id 
    [HttpGet("Get-Category-by-ID/{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var category = await _categoryService.GetCategoryByIdAsync(id);

        if (category is null)
            return NotFound(new { success = false, message = $"Category '{id}' not found" });

        return Ok(new { success = true, message = "Category retrieved successfully", data = category });
    }

    // update category by id
    [HttpPut("Update-Category-by-ID/{id}")]
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

}
