using ProductService.Application.DTOs;
using ProductService.Domain.Entities;

namespace ProductService.Application.Interfaces;

public interface ICategoryService
{
    Task<List<Category>> GetAllCategories();
    void AddCategory(Category category);
    Task<CategoryDto?> GetCategoryByIdAsync(Guid id);
    Task<CategoryDto?> UpdateCategoryAsync(Guid id, UpdateCategoryRequest request);
    Task<bool> DeleteCategoryAsync(Guid id);
}
