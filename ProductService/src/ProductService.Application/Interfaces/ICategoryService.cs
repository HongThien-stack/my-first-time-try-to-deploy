using ProductService.Application.DTOs;

namespace ProductService.Application.Interfaces;

public interface ICategoryService
{
    Task<CategoryDto?> GetCategoryByIdAsync(Guid id);
    Task<CategoryDto?> UpdateCategoryAsync(Guid id, UpdateCategoryRequest request);
    Task<bool> DeleteCategoryAsync(Guid id);
}