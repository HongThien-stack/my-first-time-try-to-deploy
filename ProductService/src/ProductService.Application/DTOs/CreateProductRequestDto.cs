using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace ProductService.Application.DTOs;

public class CreateProductRequestDto
{
    [Required(ErrorMessage = "SKU là bắt buộc")]
    [StringLength(50, ErrorMessage = "SKU không được vượt quá 50 ký tự")]
    public string Sku { get; set; } = string.Empty;

    [StringLength(50, ErrorMessage = "Barcode không được vượt quá 50 ký tự")]
    public string? Barcode { get; set; }

    [Required(ErrorMessage = "Tên sản phẩm là bắt buộc")]
    [StringLength(255, MinimumLength = 3, ErrorMessage = "Tên sản phẩm phải từ 3 đến 255 ký tự")]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Required(ErrorMessage = "Danh mục là bắt buộc")]
    public Guid CategoryId { get; set; }

    public Guid? SupplierId { get; set; }

    [StringLength(100, ErrorMessage = "Thương hiệu không được vượt quá 100 ký tự")]
    public string? Brand { get; set; }

    [StringLength(100, ErrorMessage = "Xuất xứ không được vượt quá 100 ký tự")]
    public string? Origin { get; set; }

    [Required(ErrorMessage = "Giá bán là bắt buộc")]
    [Range(0, double.MaxValue, ErrorMessage = "Giá bán phải lớn hơn hoặc bằng 0")]
    public decimal Price { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Giá gốc phải lớn hơn hoặc bằng 0")]
    public decimal? OriginalPrice { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Giá vốn phải lớn hơn hoặc bằng 0")]
    public decimal? CostPrice { get; set; }

    [Required(ErrorMessage = "Đơn vị là bắt buộc")]
    [StringLength(50, ErrorMessage = "Đơn vị không được vượt quá 50 ký tự")]
    public string Unit { get; set; } = string.Empty;

    [Range(0, double.MaxValue, ErrorMessage = "Khối lượng phải lớn hơn hoặc bằng 0")]
    public decimal? Weight { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Thể tích phải lớn hơn hoặc bằng 0")]
    public decimal? Volume { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Số lượng mỗi đơn vị phải lớn hơn 0")]
    public int QuantityPerUnit { get; set; } = 1;

    [Range(1, int.MaxValue, ErrorMessage = "Số lượng đặt tối thiểu phải lớn hơn 0")]
    public int MinOrderQuantity { get; set; } = 1;

    [Range(1, int.MaxValue, ErrorMessage = "Số lượng đặt tối đa phải lớn hơn 0")]
    public int? MaxOrderQuantity { get; set; }

    public DateTime? ExpirationDate { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Hạn sử dụng phải lớn hơn 0 ngày")]
    public int? ShelfLifeDays { get; set; }

    [StringLength(500, ErrorMessage = "Hướng dẫn bảo quản không được vượt quá 500 ký tự")]
    public string? StorageInstructions { get; set; }

    public bool IsPerishable { get; set; } = false;

    public bool IsAvailable { get; set; } = true;
    public bool IsFeatured { get; set; } = false;
    public bool IsNew { get; set; } = false;
    public bool IsOnSale { get; set; } = false;

    [StringLength(255, ErrorMessage = "Slug không được vượt quá 255 ký tự")]
    public string? Slug { get; set; }

    [StringLength(255, ErrorMessage = "Meta title không được vượt quá 255 ký tự")]
    public string? MetaTitle { get; set; }

    [StringLength(500, ErrorMessage = "Meta description không được vượt quá 500 ký tự")]
    public string? MetaDescription { get; set; }

    [StringLength(500, ErrorMessage = "Meta keywords không được vượt quá 500 ký tự")]
    public string? MetaKeywords { get; set; }

    // Ảnh chính (bắt buộc)
    [Required(ErrorMessage = "Ảnh chính là bắt buộc")]
    public IFormFile MainImage { get; set; } = null!;

    // Ảnh phụ (tối đa 10 ảnh)
    [MaxLength(10, ErrorMessage = "Tối đa 10 ảnh phụ")]
    public List<IFormFile>? AdditionalImages { get; set; }
}
