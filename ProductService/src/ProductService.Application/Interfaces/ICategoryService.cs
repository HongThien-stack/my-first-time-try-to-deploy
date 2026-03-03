using ProductService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductService.Application.Interfaces
{
    public interface ICategoryService
    {
        Task<List<Category>> GetAllCategories();
        void AddCategory(Category category);
    }
}
