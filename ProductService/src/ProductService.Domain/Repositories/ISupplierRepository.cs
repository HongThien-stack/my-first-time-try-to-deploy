using ProductService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductService.Domain.Repositories
{
    public interface ISupplierRepository
    {
        Task AddNewSupplierAsync(Supplier supplier);
        Task<Supplier?> GetSupplierBySupplierIdAsync(Guid supplierId);
        Task<List<Supplier>> GetAllSuppliersAsync();
        Task UpdateSupplierAsync(Supplier supplier);
        Task DisableSupplierAsync(Supplier supplier);
    }
}
