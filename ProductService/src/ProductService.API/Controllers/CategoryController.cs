using Microsoft.AspNetCore.Mvc;
using ProductService.Application.DTOs;
using ProductService.Application.Interfaces;
using ProductService.Domain.Entities;

namespace ProductService.API.Controllers
{
    [ApiController]
    [Route("api/product-service/category")]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _cs;
        public CategoryController(ICategoryService cs)
        {
            _cs = cs;
        }

        [HttpGet("get-all-categories")]
        public async Task<ActionResult<List<CategoryResponse>>> GetAllCategories()
        {
            List<CategoryResponse> categoryResponses = new List<CategoryResponse>();
            var categories = await _cs.GetAllCategories();
            if (categories == null || categories.Count == 0)
            {
                return NotFound("No categories found.");
            }

            foreach (var category in categories)
            {
                categoryResponses.Add(new CategoryResponse
                {
                    Id = category.Id,
                    Name = category.Name
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
            string status = "Active";
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
            _cs.AddCategory(category);
            return Ok("Category added successfully.");
        }
    }
}
