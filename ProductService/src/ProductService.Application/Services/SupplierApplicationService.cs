using ProductService.Application.DTOs;
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
    public class SupplierApplicationService : ISupplierService
    {
        private readonly ISupplierRepository _supplierRepository;
        public SupplierApplicationService(ISupplierRepository supplierRepository)
        {
            _supplierRepository = supplierRepository;
        }

        public async Task AddNewSupplierAsync(SupplierRequest supplierRequest)
        {
            Supplier supplier = new Supplier
            {
                Id = Guid.NewGuid(),
                Name = supplierRequest.Name,
                Phone = supplierRequest.Phone,
                Email = supplierRequest.Email,
                ContactPerson = supplierRequest.ContactPerson,
                Status = "ACTIVE",
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = null
            };
            await _supplierRepository.AddNewSupplierAsync(supplier);
        }

        public async Task<Supplier?> GetSupplierBySupplierIdAsync(Guid supplierId)
        {
            return await _supplierRepository.GetSupplierBySupplierIdAsync(supplierId);
        }

        public async Task<List<Supplier>> GetAllSuppliersAsync()
        {
            return await _supplierRepository.GetAllSuppliersAsync();
        }

        public async Task UpdateSupplierAsync(Supplier supplier, SupplierRequest supplierRequest)
        {
            if (!string.IsNullOrEmpty(supplierRequest.Name))
            {
                supplier.Name = supplierRequest.Name;
            }
            if (!string.IsNullOrEmpty(supplierRequest.Phone))
            {
                supplier.Phone = supplierRequest.Phone;
            }
            if (!string.IsNullOrEmpty(supplierRequest.Email))
            {
                supplier.Email = supplierRequest.Email;
            }
            if (!string.IsNullOrEmpty(supplierRequest.ContactPerson))
            {
                supplier.ContactPerson = supplierRequest.ContactPerson;
            }
            supplier.UpdatedAt = DateTime.UtcNow;
            await _supplierRepository.UpdateSupplierAsync(supplier);
        }

        public async Task DisableSupplierAsync(Supplier supplier)
        {
            await _supplierRepository.DisableSupplierAsync(supplier);
        }
    }
}
