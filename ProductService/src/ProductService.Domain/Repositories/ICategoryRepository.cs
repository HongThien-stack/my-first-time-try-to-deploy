using ProductService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductService.Domain.Repositories;

public interface ICategoryRepository
{
    Task<List<Category>> GetAllCategories();
    void AddCategory(Category category);
    Task<Category?> GetByIdAsync(Guid id);
    Task<Category> UpdateAsync(Category category);

}