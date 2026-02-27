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

    public async Task<ProductDto?> UpdateProductAsync(Guid id, UpdateProductRequestDto request)
    {
        var product = await _productRepository.GetByIdAsync(id);
        if (product == null)
            return null;

        // ===================== BUSINESS RULES =====================

        // 1. Tên không được để trống nếu được gửi lên
        if (request.Name is not null && string.IsNullOrWhiteSpace(request.Name))
            throw new ArgumentException("Tên sản phẩm không được để trống.");

        // 2. Giá bán phải lớn hơn 0
        if (request.Price.HasValue && request.Price.Value <= 0)
            throw new ArgumentException("Giá bán phải lớn hơn 0.");

        var effectivePrice         = request.Price ?? product.Price;
        var effectiveOriginalPrice = request.OriginalPrice ?? product.OriginalPrice;
        var effectiveCostPrice     = request.CostPrice ?? product.CostPrice;

        // 3. Giá gốc phải < giá bán
        //    (original_price = giá gốc/giá tham chiếu, luôn thấp hơn giá bán ra thị trường)
        if (effectiveOriginalPrice.HasValue && effectiveOriginalPrice.Value >= effectivePrice)
            throw new ArgumentException($"Giá gốc ({effectiveOriginalPrice.Value:N0}đ) phải nhỏ hơn giá bán ({effectivePrice:N0}đ).");

        // 4. Giá vốn phải < giá gốc (nếu có) và < giá bán
        //    (cost_price = giá vốn nhập kho, luôn thấp nhất)
        if (effectiveCostPrice.HasValue)
        {
            if (effectiveCostPrice.Value <= 0)
                throw new ArgumentException("Giá vốn phải lớn hơn 0.");

            if (effectiveOriginalPrice.HasValue && effectiveCostPrice.Value >= effectiveOriginalPrice.Value)
                throw new ArgumentException($"Giá vốn ({effectiveCostPrice.Value:N0}đ) phải nhỏ hơn giá gốc ({effectiveOriginalPrice.Value:N0}đ).");

            if (effectiveCostPrice.Value >= effectivePrice)
                throw new ArgumentException($"Giá vốn ({effectiveCostPrice.Value:N0}đ) phải nhỏ hơn giá bán ({effectivePrice:N0}đ).");
        }

        // 5. IsOnSale = true bắt buộc phải có giá gốc (để hiển thị "giá gốc gạch ngang")
        var effectiveIsOnSale = request.IsOnSale ?? product.IsOnSale;
        if (effectiveIsOnSale && !effectiveOriginalPrice.HasValue)
            throw new ArgumentException("Sản phẩm đang khuyến mãi (IsOnSale = true) phải có giá gốc (OriginalPrice).");

        // 6. Số lượng tối đa phải >= tối thiểu
        var effectiveMin = request.MinOrderQuantity ?? product.MinOrderQuantity;
        var effectiveMax = request.MaxOrderQuantity ?? product.MaxOrderQuantity;
        if (effectiveMax.HasValue && effectiveMax.Value < effectiveMin)
            throw new ArgumentException($"Số lượng đặt tối đa ({effectiveMax.Value}) phải >= tối thiểu ({effectiveMin}).");

        // 7. Hạn sử dụng phải ở tương lai
        if (request.ExpirationDate.HasValue && request.ExpirationDate.Value.Date <= DateTime.UtcNow.Date)
            throw new ArgumentException("Hạn sử dụng phải là ngày trong tương lai.");

        // ===================== ÁP DỤNG THAY ĐỔI =====================

        if (request.Name is not null)                product.Name = request.Name.Trim();
        if (request.Barcode is not null)             product.Barcode = request.Barcode.Trim();
        if (request.Description is not null)         product.Description = request.Description.Trim();
        if (request.CategoryId.HasValue)             product.CategoryId = request.CategoryId.Value;
        if (request.Brand is not null)               product.Brand = request.Brand.Trim();
        if (request.Origin is not null)              product.Origin = request.Origin.Trim();
        if (request.Price.HasValue)                  product.Price = request.Price.Value;
        if (request.OriginalPrice.HasValue)          product.OriginalPrice = request.OriginalPrice;
        if (request.CostPrice.HasValue)              product.CostPrice = request.CostPrice;
        if (request.Unit is not null)                product.Unit = request.Unit.Trim();
        if (request.Weight.HasValue)                 product.Weight = request.Weight;
        if (request.Volume.HasValue)                 product.Volume = request.Volume;
        if (request.QuantityPerUnit.HasValue)        product.QuantityPerUnit = request.QuantityPerUnit.Value;
        if (request.MinOrderQuantity.HasValue)       product.MinOrderQuantity = request.MinOrderQuantity.Value;
        if (request.MaxOrderQuantity.HasValue)       product.MaxOrderQuantity = request.MaxOrderQuantity;
        if (request.ExpirationDate.HasValue)         product.ExpirationDate = request.ExpirationDate;
        if (request.ShelfLifeDays.HasValue)          product.ShelfLifeDays = request.ShelfLifeDays;
        if (request.StorageInstructions is not null) product.StorageInstructions = request.StorageInstructions.Trim();
        if (request.IsPerishable.HasValue)           product.IsPerishable = request.IsPerishable.Value;
        if (request.ImageUrl is not null)            product.ImageUrl = request.ImageUrl.Trim();
        if (request.Images is not null)              product.Images = request.Images;
        if (request.IsAvailable.HasValue)            product.IsAvailable = request.IsAvailable.Value;
        if (request.IsFeatured.HasValue)             product.IsFeatured = request.IsFeatured.Value;
        if (request.IsNew.HasValue)                  product.IsNew = request.IsNew.Value;
        if (request.IsOnSale.HasValue)               product.IsOnSale = request.IsOnSale.Value;
        if (request.Slug is not null)                product.Slug = request.Slug.Trim().ToLower();
        if (request.MetaTitle is not null)           product.MetaTitle = request.MetaTitle.Trim();
        if (request.MetaDescription is not null)     product.MetaDescription = request.MetaDescription.Trim();
        if (request.MetaKeywords is not null)        product.MetaKeywords = request.MetaKeywords.Trim();

        product.UpdatedAt = DateTime.UtcNow;
        product.UpdatedBy = Guid.TryParse(request.UpdatedBy, out var updatedByGuid) ? updatedByGuid : null;

        var updated = await _productRepository.UpdateAsync(product);

        return new ProductDto
        {
            Id = updated.Id,
            Sku = updated.Sku,
            Barcode = updated.Barcode,
            Name = updated.Name,
            Description = updated.Description,
            CategoryId = updated.CategoryId,
            CategoryName = updated.Category?.Name,
            Brand = updated.Brand,
            Origin = updated.Origin,
            Price = updated.Price,
            OriginalPrice = updated.OriginalPrice,
            Unit = updated.Unit,
            Weight = updated.Weight,
            Volume = updated.Volume,
            IsAvailable = updated.IsAvailable,
            IsFeatured = updated.IsFeatured,
            IsNew = updated.IsNew,
            IsOnSale = updated.IsOnSale,
            ImageUrl = updated.ImageUrl,
            Slug = updated.Slug,
            CreatedAt = updated.CreatedAt
        };
    }

    public async Task<bool> DeleteProductAsync(Guid id)
    {
        var product = await _productRepository.GetByIdAsync(id);
        if (product == null)
            return false;

        // Soft delete: ẩn sản phẩm khỏi danh sách
        product.IsAvailable = false;
        product.IsDeleted = true;
        product.UpdatedAt = DateTime.UtcNow;

        await _productRepository.SoftDeleteAsync(product);
        return true;
    }
}
