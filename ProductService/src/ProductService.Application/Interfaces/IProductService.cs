using ProductService.Application.DTOs;

namespace ProductService.Application.Interfaces;

public interface IProductService
{
    Task<IEnumerable<ProductDto>> GetAllProductsAsync();
    Task<CreateProductResponseDto> CreateProductAsync(
        CreateProductRequestDto request, 
        Guid userId);
    Task<ProductDto?> GetProductByIdAsync(Guid id);
    Task<ProductDto?> UpdateProductAsync(Guid id, UpdateProductRequestDto request);
    Task<bool> DeleteProductAsync(Guid id);
}
