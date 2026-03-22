namespace ProductService.Domain.Entities;

public class Product
{
    public Guid Id { get; set; }
    
    // Thông tin cơ bản
    public string Sku { get; set; } = string.Empty;
    public string? Barcode { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    
    // Phân loại
    public Guid CategoryId { get; set; }
    public Guid? SupplierId { get; set; }
    public string? Brand { get; set; }
    public string? Origin { get; set; }
    
    // Giá và khuyến mãi
    public decimal Price { get; set; }
    public decimal? OriginalPrice { get; set; }
    public decimal? CostPrice { get; set; }
    
    // Đơn vị và khối lượng
    public string Unit { get; set; } = string.Empty;
    public decimal? Weight { get; set; }
    public decimal? Volume { get; set; }
    public int QuantityPerUnit { get; set; } = 1;
    
    // Đặt hàng
    public int MinOrderQuantity { get; set; } = 1;
    public int? MaxOrderQuantity { get; set; }
    
    // Hạn sử dụng và bảo quản
    public DateTime? ExpirationDate { get; set; }
    public int? ShelfLifeDays { get; set; }
    public string? StorageInstructions { get; set; }
    public bool IsPerishable { get; set; } = false;
    
    // Hình ảnh
    public string? ImageUrl { get; set; }
    public string? Images { get; set; }
    
    // Trạng thái
    public bool IsAvailable { get; set; } = true;
    public bool IsFeatured { get; set; } = false;
    public bool IsNew { get; set; } = false;
    public bool IsOnSale { get; set; } = false;
    public bool IsDeleted { get; set; } = false;
    
    // SEO
    public string? Slug { get; set; }
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
    public string? MetaKeywords { get; set; }
    
    // Audit
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Guid? CreatedBy { get; set; }
    public Guid? UpdatedBy { get; set; }
    
    // Navigation properties
    public Category Category { get; set; } = null!;
    public Supplier? Supplier { get; set; }
}
