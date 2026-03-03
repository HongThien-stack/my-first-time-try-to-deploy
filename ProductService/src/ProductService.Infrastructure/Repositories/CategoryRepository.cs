using Microsoft.EntityFrameworkCore;
using ProductService.Domain.Entities;
using ProductService.Domain.Repositories;
using ProductService.Infrastructure.Data;

namespace ProductService.Infrastructure.Repositories;

public class CategoryRepository : ICategoryRepository
{
    private readonly ProductDbContext _context;

    public CategoryRepository(ProductDbContext context)
    {
        _context = context;
    }

    public async Task<List<Category>> GetAllCategories()
    {
        return await _context.Categories
            .Where(c => !c.IsDeleted)
            .ToListAsync();
    }

    public void AddCategory(Category category)
    {
        _context.Categories.Add(category);
        _context.SaveChanges();
    }

    public async Task<Category?> GetByIdAsync(Guid id)
    {
        return await _context.Categories
            .Include(c => c.Products)
            .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);
    }

    public async Task<Category> UpdateAsync(Category category)
    {
        _context.Categories.Update(category);
        await _context.SaveChangesAsync();
        return category;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var category = await _context.Categories
            .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);
        if (category == null)
            return false;

        // Soft delete: chỉ cập nhật IsDeleted = true
        category.IsDeleted = true;
        category.UpdatedAt = DateTime.UtcNow;
        
        _context.Categories.Update(category);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> HasProductsAsync(Guid id)
    {
        return await _context.Products.AnyAsync(p => p.CategoryId == id && !p.IsDeleted);
    }
}
