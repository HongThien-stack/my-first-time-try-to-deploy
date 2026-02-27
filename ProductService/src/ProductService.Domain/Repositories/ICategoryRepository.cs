using ProductService.Domain.Entities;

namespace ProductService.Domain.Repositories;

public interface ICategoryRepository
{
    Task<Category?> GetByIdAsync(Guid id);
    Task<Category> UpdateAsync(Category category);
}