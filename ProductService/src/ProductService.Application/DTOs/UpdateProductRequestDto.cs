using System.ComponentModel.DataAnnotations;

namespace ProductService.Application.DTOs;

public class UpdateProductRequestDto
{
    // Thông tin cơ bản
    [StringLength(50, ErrorMessage = "Barcode không được vượt quá 50 ký tự")]
    public string? Barcode { get; set; }

    [StringLength(255, MinimumLength = 2, ErrorMessage = "Tên sản phẩm từ 2 đến 255 ký tự")]
    public string? Name { get; set; }

    public string? Description { get; set; }

    // Phân loại
    public Guid? CategoryId { get; set; }

    [StringLength(100, ErrorMessage = "Thương hiệu không được vượt quá 100 ký tự")]
    public string? Brand { get; set; }

    [StringLength(100, ErrorMessage = "Xuất xứ không được vượt quá 100 ký tự")]
    public string? Origin { get; set; }

    // Giá
    [Range(0.01, double.MaxValue, ErrorMessage = "Giá bán phải lớn hơn 0")]
    public decimal? Price { get; set; }

    [Range(0.01, double.MaxValue, ErrorMessage = "Giá gốc phải lớn hơn 0")]
    public decimal? OriginalPrice { get; set; }

    [Range(0.01, double.MaxValue, ErrorMessage = "Giá vốn phải lớn hơn 0")]
    public decimal? CostPrice { get; set; }

    // Đơn vị và khối lượng
    [StringLength(50, ErrorMessage = "Đơn vị không được vượt quá 50 ký tự")]
    public string? Unit { get; set; }

    [Range(0.001, double.MaxValue, ErrorMessage = "Khối lượng phải lớn hơn 0")]
    public decimal? Weight { get; set; }

    [Range(0.001, double.MaxValue, ErrorMessage = "Thể tích phải lớn hơn 0")]
    public decimal? Volume { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Số lượng mỗi đơn vị phải >= 1")]
    public int? QuantityPerUnit { get; set; }

    // Đặt hàng
    [Range(1, int.MaxValue, ErrorMessage = "Số lượng đặt tối thiểu phải >= 1")]
    public int? MinOrderQuantity { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Số lượng đặt tối đa phải >= 1")]
    public int? MaxOrderQuantity { get; set; }

    // Hạn sử dụng và bảo quản
    public DateTime? ExpirationDate { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Hạn sử dụng (ngày) phải >= 1")]
    public int? ShelfLifeDays { get; set; }

    [StringLength(500, ErrorMessage = "Hướng dẫn bảo quản không được vượt quá 500 ký tự")]
    public string? StorageInstructions { get; set; }

    public bool? IsPerishable { get; set; }

    // Hình ảnh
    [StringLength(500, ErrorMessage = "URL ảnh không được vượt quá 500 ký tự")]
    public string? ImageUrl { get; set; }

    public string? Images { get; set; }

    // Trạng thái
    public bool? IsAvailable { get; set; }
    public bool? IsFeatured { get; set; }
    public bool? IsNew { get; set; }
    public bool? IsOnSale { get; set; }

    // SEO
    [StringLength(255, ErrorMessage = "Slug không được vượt quá 255 ký tự")]
    [RegularExpression(@"^[a-z0-9]+(?:-[a-z0-9]+)*$", ErrorMessage = "Slug chỉ chứa chữ thường, số và dấu gạch nông (ví dụ: san-pham-moi)")]
    public string? Slug { get; set; }

    [StringLength(255, ErrorMessage = "Meta title không được vượt quá 255 ký tự")]
    public string? MetaTitle { get; set; }

    [StringLength(500, ErrorMessage = "Meta description không được vượt quá 500 ký tự")]
    public string? MetaDescription { get; set; }

    [StringLength(500, ErrorMessage = "Meta keywords không được vượt quá 500 ký tự")]
    public string? MetaKeywords { get; set; }

    // Ai thực hiện update (truyền GUID dạng chuỗi hoặc để null)
    public string? UpdatedBy { get; set; }
}
