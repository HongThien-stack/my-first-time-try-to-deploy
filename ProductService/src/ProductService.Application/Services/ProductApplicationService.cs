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
    private readonly IProductAuditLogRepository _auditLogRepository;
    private readonly ICloudinaryService _cloudinaryService;
    private readonly ILogger<ProductApplicationService> _logger;

    public ProductApplicationService(
        IProductRepository productRepository,
        IProductAuditLogRepository auditLogRepository,
        ICloudinaryService cloudinaryService,
        ILogger<ProductApplicationService> logger)
    {
        _productRepository = productRepository;
        _auditLogRepository = auditLogRepository;
        _cloudinaryService = cloudinaryService;
        _logger = logger;
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

    public async Task<CreateProductResponseDto> CreateProductAsync(
        CreateProductRequestDto request, 
        Guid userId, 
        string userName, 
        string? ipAddress)
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
        if (request.OriginalPrice.HasValue && request.OriginalPrice < request.Price)
        {
            throw new ArgumentException("Giá gốc không thể nhỏ hơn giá bán");
        }

        if (request.CostPrice.HasValue && request.CostPrice > request.Price)
        {
            _logger.LogWarning("Cost price ({CostPrice}) is higher than selling price ({Price})", 
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

            // Generate slug if not provided
            var slug = request.Slug ?? GenerateSlug(request.Name);

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

            // Create audit log
            var auditLog = new ProductAuditLog
            {
                ProductId = createdProduct.Id,
                PerformedBy = userId,
                PerformedByName = userName,
                Action = "CREATE",
                NewValues = JsonSerializer.Serialize(new
                {
                    createdProduct.Sku,
                    createdProduct.Name,
                    createdProduct.Price,
                    createdProduct.CategoryId,
                    createdProduct.ImageUrl,
                    ImagesCount = additionalImageUrls?.Count ?? 0
                }),
                Description = $"Tạo sản phẩm mới: {createdProduct.Name}",
                IpAddress = ipAddress,
                PerformedAt = DateTime.UtcNow
            };

            await _auditLogRepository.CreateAsync(auditLog);

            _logger.LogInformation(
                "Product created successfully: {ProductId} - {ProductName} by {UserName}", 
                createdProduct.Id, createdProduct.Name, userName);

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
                CreatedBy = createdProduct.CreatedBy,
                CreatedByName = userName
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
}
