using ProductService.Application.DTOs;
using ProductService.Application.Interfaces;
using ProductService.Domain.Entities;
using ProductService.Domain.Repositories;

namespace ProductService.Application.Services;

// Application để xử lý logic liên quan đến Category, bao gồm lấy thông tin category và cập nhật category.

public class CategoryApplicationService : ICategoryService
{
    private readonly ICategoryRepository _categoryRepository;

    public CategoryApplicationService(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<List<Category>> GetAllCategories()
    {
        return await _categoryRepository.GetAllCategories();
    }

    public void AddCategory(Category category)
    {
        _categoryRepository.AddCategory(category);
    }

    public async Task<CategoryDto?> GetCategoryByIdAsync(Guid id)
    {
        var category = await _categoryRepository.GetByIdAsync(id);

        if (category is null)
            return null;

        return new CategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Status = category.Status,
            IsDeleted = category.IsDeleted,
            ProductCount = category.Products.Count,
            CreatedAt = category.CreatedAt,
            UpdatedAt = category.UpdatedAt
        };
    }

    public async Task<CategoryDto?> UpdateCategoryAsync(Guid id, UpdateCategoryRequest request)
    {
        var category = await _categoryRepository.GetByIdAsync(id);

        if (category is null)
            return null;

        category.Name = request.Name;
        category.Status = request.Status;
        category.IsDeleted = request.IsDeleted;
        category.UpdatedAt = DateTime.UtcNow;

        var updated = await _categoryRepository.UpdateAsync(category);

        return new CategoryDto
        {
            Id = updated.Id,
            Name = updated.Name,
            Status = updated.Status,
            IsDeleted = updated.IsDeleted,
            ProductCount = updated.Products.Count,
            CreatedAt = updated.CreatedAt,
            UpdatedAt = updated.UpdatedAt
        };
    }

}
