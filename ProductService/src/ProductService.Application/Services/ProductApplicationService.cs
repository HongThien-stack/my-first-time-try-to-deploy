using System.Text.Json;
using Microsoft.Extensions.Logging;
using ProductService.Application.DTOs;
using ProductService.Application.Interfaces;
using ProductService.Domain.Entities;
using ProductService.Domain.Repositories;

namespace ProductService.Application.Services;

public class ProductApplicationService : IProductService
{
    private readonly IProductRepository _productRepository;
    private readonly ICloudinaryService _cloudinaryService;
    private readonly ILogger<ProductApplicationService> _logger;

    public ProductApplicationService(
        IProductRepository productRepository,
        ICloudinaryService cloudinaryService,
        ILogger<ProductApplicationService> logger)
    {
        _productRepository = productRepository;
        _cloudinaryService = cloudinaryService;
        _logger = logger;
    }

    public async Task<IEnumerable<ProductDetailsDto>> GetProductDetailsBatchAsync(IEnumerable<Guid> productIds)
    {
        if (productIds == null || !productIds.Any())
        {
            return Enumerable.Empty<ProductDetailsDto>();
        }

        var products = await _productRepository.GetByIdsAsync(productIds);

        return products.Select(p => new ProductDetailsDto
        {
            Id = p.Id,
            Name = p.Name,
            Price = p.Price,
            CategoryId = p.CategoryId,
            CategoryName = p.Category?.Name ?? string.Empty
        });
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
            Images = p.Images,
            Slug = p.Slug,
            CreatedAt = p.CreatedAt
        });
    }

    public async Task<CreateProductResponseDto> CreateProductAsync(
        CreateProductRequestDto request, 
        Guid userId)
    {
        // Validate SKU uniqueness
        if (await _productRepository.ExistsBySkuAsync(request.Sku))
        {
            throw new ArgumentException($"SKU '{request.Sku}' đã tồn tại");
        }

        // Validate Barcode uniqueness
        if (!string.IsNullOrEmpty(request.Barcode) && 
            await _productRepository.ExistsByBarcodeAsync(request.Barcode))
        {
            throw new ArgumentException($"Barcode '{request.Barcode}' đã tồn tại");
        }

        // Validate pricing logic
        if (request.OriginalPrice.HasValue && request.OriginalPrice >= request.Price)
        {
            throw new ArgumentException("Giá gốc = giá bán hoặc giá gốc lớn hơn giá bán  ");
        }

        if (request.CostPrice.HasValue && request.CostPrice < request.OriginalPrice)
        {
            _logger.LogWarning("giá vốn phải nhỏ hơn giá gốc", 
                request.CostPrice, request.Price);
        }

        // Validate order quantities
        if (request.MaxOrderQuantity.HasValue && 
            request.MaxOrderQuantity < request.MinOrderQuantity)
        {
            throw new ArgumentException("Số lượng đặt tối đa không thể nhỏ hơn số lượng đặt tối thiểu");
        }

        string? mainImageUrl = null;
        List<string>? additionalImageUrls = null;

        try
        {
            // Upload main image
            _logger.LogInformation("Uploading main image for product {Sku}", request.Sku);
            mainImageUrl = await _cloudinaryService.UploadImageAsync(request.MainImage, "products");

            // Upload additional images
            if (request.AdditionalImages != null && request.AdditionalImages.Any())
            {
                _logger.LogInformation("Uploading {Count} additional images", request.AdditionalImages.Count);
                additionalImageUrls = await _cloudinaryService.UploadImagesAsync(
                    request.AdditionalImages, "products");
            }

            // Generate slug if not provided or invalid
            var slug = string.IsNullOrWhiteSpace(request.Slug) || request.Slug.ToLower() == "string" 
                ? GenerateUniqueSlug(request.Name) 
                : request.Slug;

            // Create product entity
            var product = new Product
            {
                Id = Guid.NewGuid(),
                Sku = request.Sku,
                Barcode = request.Barcode,
                Name = request.Name,
                Description = request.Description,
                CategoryId = request.CategoryId,
                Brand = request.Brand,
                Origin = request.Origin,
                Price = request.Price,
                OriginalPrice = request.OriginalPrice,
                CostPrice = request.CostPrice,
                Unit = request.Unit,
                Weight = request.Weight,
                Volume = request.Volume,
                QuantityPerUnit = request.QuantityPerUnit,
                MinOrderQuantity = request.MinOrderQuantity,
                MaxOrderQuantity = request.MaxOrderQuantity,
                ExpirationDate = request.ExpirationDate,
                ShelfLifeDays = request.ShelfLifeDays,
                StorageInstructions = request.StorageInstructions,
                IsPerishable = request.IsPerishable,
                ImageUrl = mainImageUrl,
                Images = additionalImageUrls != null && additionalImageUrls.Any() 
                    ? JsonSerializer.Serialize(additionalImageUrls) 
                    : null,
                IsAvailable = request.IsAvailable,
                IsFeatured = request.IsFeatured,
                IsNew = request.IsNew,
                IsOnSale = request.IsOnSale,
                Slug = slug,
                MetaTitle = request.MetaTitle,
                MetaDescription = request.MetaDescription,
                MetaKeywords = request.MetaKeywords,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = userId
            };

            // Save product to database
            var createdProduct = await _productRepository.CreateAsync(product);

            _logger.LogInformation(
                "Product created successfully: {ProductId} - {ProductName}", 
                createdProduct.Id, createdProduct.Name);

            // Return response
            return new CreateProductResponseDto
            {
                Id = createdProduct.Id,
                Sku = createdProduct.Sku,
                Name = createdProduct.Name,
                Barcode = createdProduct.Barcode,
                CategoryId = createdProduct.CategoryId,
                CategoryName = createdProduct.Category?.Name,
                Price = createdProduct.Price,
                Unit = createdProduct.Unit,
                Brand = createdProduct.Brand,
                Origin = createdProduct.Origin,
                ImageUrl = createdProduct.ImageUrl,
                Images = additionalImageUrls,
                Slug = createdProduct.Slug,
                CreatedAt = createdProduct.CreatedAt,
                CreatedBy = createdProduct.CreatedBy
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating product {Sku}", request.Sku);

            // Rollback: Delete uploaded images
            if (mainImageUrl != null)
            {
                _logger.LogWarning("Rolling back uploaded images due to error");
                var imagesToDelete = new List<string> { mainImageUrl };
                if (additionalImageUrls != null)
                {
                    imagesToDelete.AddRange(additionalImageUrls);
                }

                // Best effort deletion - don't throw if this fails
                try
                {
                    await Task.WhenAll(imagesToDelete.Select(url => 
                        _cloudinaryService.DeleteImageAsync(GetPublicIdFromUrl(url))));
                }
                catch (Exception deleteEx)
                {
                    _logger.LogError(deleteEx, "Error during image rollback");
                }
            }

            throw;
        }
    }

    private string GenerateUniqueSlug(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return $"product-{Guid.NewGuid().ToString().Substring(0, 8)}";

        // Convert to lowercase
        var slug = name.ToLowerInvariant();

        // Remove Vietnamese diacritics
        slug = RemoveVietnameseDiacritics(slug);

        // Replace spaces with hyphens
        slug = slug.Replace(' ', '-');

        // Remove special characters
        slug = new string(slug.Where(c => char.IsLetterOrDigit(c) || c == '-').ToArray());

        // Remove consecutive hyphens
        while (slug.Contains("--"))
        {
            slug = slug.Replace("--", "-");
        }

        slug = slug.Trim('-');
        
        // Add timestamp suffix to ensure uniqueness
        var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        return $"{slug}-{timestamp}";
    }

    private string GenerateSlug(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return string.Empty;

        // Convert to lowercase
        var slug = name.ToLowerInvariant();

        // Remove Vietnamese diacritics
        slug = RemoveVietnameseDiacritics(slug);

        // Replace spaces with hyphens
        slug = slug.Replace(' ', '-');

        // Remove special characters
        slug = new string(slug.Where(c => char.IsLetterOrDigit(c) || c == '-').ToArray());

        // Remove consecutive hyphens
        while (slug.Contains("--"))
        {
            slug = slug.Replace("--", "-");
        }

        return slug.Trim('-');
    }

    private string RemoveVietnameseDiacritics(string text)
    {
        var vietnameseChars = "àáạảãâầấậẩẫăằắặẳẵèéẹẻẽêềấệểễìíịỉĩòóọỏõôồốộổỗơờớợởỡùúụủũưừứựửữỳýỵỷỹđ";
        var replacementChars = "aaaaaaaaaaaaaaaaaeeeeeeeeeeeiiiiiooooooooooooooooouuuuuuuuuuuyyyyyđ";

        for (int i = 0; i < vietnameseChars.Length; i++)
        {
            text = text.Replace(vietnameseChars[i], replacementChars[i]);
        }

        return text;
    }

    private string GetPublicIdFromUrl(string url)
    {
        // Extract public_id from Cloudinary URL
        var uri = new Uri(url);
        var segments = uri.AbsolutePath.Split('/');
        var uploadIndex = Array.IndexOf(segments, "upload");
        
        if (uploadIndex == -1 || uploadIndex >= segments.Length - 2)
        {
            return string.Empty;
        }
        
        var publicIdParts = segments.Skip(uploadIndex + 2).ToArray();
        var publicId = string.Join("/", publicIdParts);
        
        return Path.GetFileNameWithoutExtension(publicId);
    }

    public async Task<ProductDto?> GetProductByIdAsync(Guid id)
    {
        var product = await _productRepository.GetByIdAsync(id);
        
        if (product == null)
            return null;

        return new ProductDto
        {
            Id = product.Id,
            Sku = product.Sku,
            Barcode = product.Barcode,
            Name = product.Name,
            Description = product.Description,
            CategoryId = product.CategoryId,
            CategoryName = product.Category?.Name,
            Brand = product.Brand,
            Origin = product.Origin,
            Price = product.Price,
            OriginalPrice = product.OriginalPrice,
            Unit = product.Unit,
            Weight = product.Weight,
            Volume = product.Volume,
            IsAvailable = product.IsAvailable,
            IsFeatured = product.IsFeatured,
            IsNew = product.IsNew,
            IsOnSale = product.IsOnSale,
            ImageUrl = product.ImageUrl,
            Images = product.Images,
            Slug = product.Slug,
            CreatedAt = product.CreatedAt
        };
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
            Images = updated.Images,
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
