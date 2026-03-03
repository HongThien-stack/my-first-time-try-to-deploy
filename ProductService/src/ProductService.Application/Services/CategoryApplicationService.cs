using ProductService.Application.Interfaces;
using ProductService.Domain.Entities;
using ProductService.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductService.Application.Services
{
    public class CategoryApplicationService : ICategoryService
    {
        private readonly ICategoryRepository _cr;
        public CategoryApplicationService(ICategoryRepository cr)
        {
            _cr = cr;
        }
        public async Task<List<Category>> GetAllCategories()
        {
            return await _cr.GetAllCategories();
        }
        public void AddCategory(Category category)
        {
            _cr.AddCategory(category);
        }
    }
}
