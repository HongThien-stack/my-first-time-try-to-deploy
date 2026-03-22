using Microsoft.EntityFrameworkCore;
using ProductService.Domain.Entities;
using ProductService.Domain.Repositories;
using ProductService.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductService.Infrastructure.Repositories
{
    public class SupplierRepository : ISupplierRepository
    {
        private readonly ProductDbContext _context;
        public SupplierRepository(ProductDbContext context)
        {
            _context = context;
        }

        public async Task AddNewSupplierAsync(Supplier supplier)
        {
            await _context.Suppliers.AddAsync(supplier);
            await _context.SaveChangesAsync();
        }

        public async Task<Supplier?> GetSupplierBySupplierIdAsync(Guid supplierId)
        {
            return await _context.Suppliers.FirstOrDefaultAsync(s => s.Id == supplierId);
        }

        public async Task<List<Supplier>> GetAllSuppliersAsync()
        {
            return await _context.Suppliers.ToListAsync();
        }

        public async Task UpdateSupplierAsync(Supplier supplier)
        {
            _context.Suppliers.Update(supplier);
            await _context.SaveChangesAsync();
        }

        public async Task DisableSupplierAsync(Supplier supplier)
        {
            supplier.IsDeleted = true;
            _context.Suppliers.Update(supplier);
            await _context.SaveChangesAsync();
        }
    }
}
