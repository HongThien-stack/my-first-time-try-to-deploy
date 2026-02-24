using ProductService.Application.DTOs;

namespace ProductService.Application.Interfaces;

public interface IProductService
{
    Task<IEnumerable<ProductDto>> GetAllProductsAsync();
    Task<ProductDto?> UpdateProductAsync(Guid id, UpdateProductRequestDto request);
    Task<bool> DeleteProductAsync(Guid id);
}
