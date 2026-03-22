using ProductService.Application.DTOs;
using ProductService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductService.Application.Interfaces
{
    public interface ISupplierService
    {
        Task AddNewSupplierAsync(SupplierRequest supplierRequest);
        Task<Supplier?> GetSupplierBySupplierIdAsync(Guid supplierId);
        Task<List<Supplier>> GetAllSuppliersAsync();
        Task UpdateSupplierAsync(Supplier supplier, SupplierRequest supplierRequest);
        Task DisableSupplierAsync(Supplier supplier);
    }
}
