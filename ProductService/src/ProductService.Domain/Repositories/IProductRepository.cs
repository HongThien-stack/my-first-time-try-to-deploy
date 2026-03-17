using ProductService.Domain.Entities;

namespace ProductService.Domain.Repositories;

public interface IProductRepository
{
    Task<IEnumerable<Product>> GetAllAsync();
    Task<IEnumerable<Product>> GetByIdsAsync(IEnumerable<Guid> ids);
    Task<Product?> GetByIdAsync(Guid id);
    Task<Product?> GetBySkuAsync(string sku);
    Task<Product> CreateAsync(Product product);
    Task<Product> UpdateAsync(Product product);
    Task<bool> ExistsBySkuAsync(string sku);
    Task<bool> ExistsByBarcodeAsync(string barcode);
    Task SoftDeleteAsync(Product product);
}
