using ProductService.Application.DTOs;
using ProductService.Application.Interfaces;
using ProductService.Domain.Repositories;

namespace ProductService.Application.Services;

public class ProductApplicationService : IProductService
{
    private readonly IProductRepository _productRepository;

    public ProductApplicationService(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<IEnumerable<ProductDto>> GetAllProductsAsync()
    {
        var products = await _productRepository.GetAllAsync();
        
        return products.Select(p => new ProductDto
        {
            Id = p.Id,
            Sku = p.Sku,
            Barcode = p.Barcode,
            Name = p.Name,
            Description = p.Description,
            CategoryId = p.CategoryId,
            CategoryName = p.Category?.Name,
            Brand = p.Brand,
            Origin = p.Origin,
            Price = p.Price,
            OriginalPrice = p.OriginalPrice,
            Unit = p.Unit,
            Weight = p.Weight,
            Volume = p.Volume,
            IsAvailable = p.IsAvailable,
            IsFeatured = p.IsFeatured,
            IsNew = p.IsNew,
            IsOnSale = p.IsOnSale,
            ImageUrl = p.ImageUrl,
            Slug = p.Slug,
            CreatedAt = p.CreatedAt
        });
    }
}
