using Microsoft.EntityFrameworkCore;
using ProductService.Domain.Entities;

namespace ProductService.Infrastructure.Data;

public class ProductDbContext : DbContext
{
    public ProductDbContext(DbContextOptions<ProductDbContext> options) : base(options)
    {
    }

    public DbSet<Product> Products { get; set; }
    public DbSet<Category> Categories { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Category
        modelBuilder.Entity<Category>(entity =>
        {
            entity.ToTable("categories");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(255).IsRequired();
            entity.Property(e => e.Status).HasColumnName("status").HasMaxLength(50).IsRequired().HasDefaultValue("ACTIVE");
            entity.Property(e => e.IsDeleted).HasColumnName("is_deleted").IsRequired().HasDefaultValue(false);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").IsRequired();
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

            entity.HasIndex(e => e.Name).HasDatabaseName("IX_categories_name");
            entity.HasIndex(e => e.Status).HasDatabaseName("IX_categories_status");
            entity.HasIndex(e => e.IsDeleted).HasDatabaseName("IX_categories_is_deleted");
        });

        // Configure Product
        modelBuilder.Entity<Product>(entity =>
        {
            entity.ToTable("products");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            
            // Thông tin cơ bản
            entity.Property(e => e.Sku).HasColumnName("sku").HasMaxLength(50).IsRequired();
            entity.Property(e => e.Barcode).HasColumnName("barcode").HasMaxLength(50);
            entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(255).IsRequired();
            entity.Property(e => e.Description).HasColumnName("description");
            
            // Phân loại
            entity.Property(e => e.CategoryId).HasColumnName("category_id").IsRequired();
            entity.Property(e => e.Brand).HasColumnName("brand").HasMaxLength(100);
            entity.Property(e => e.Origin).HasColumnName("origin").HasMaxLength(100);
            
            // Giá và khuyến mãi
            entity.Property(e => e.Price).HasColumnName("price").HasColumnType("decimal(18,2)").IsRequired();
            entity.Property(e => e.OriginalPrice).HasColumnName("original_price").HasColumnType("decimal(18,2)");
            entity.Property(e => e.CostPrice).HasColumnName("cost_price").HasColumnType("decimal(18,2)");
            
            // Đơn vị và khối lượng
            entity.Property(e => e.Unit).HasColumnName("unit").HasMaxLength(50).IsRequired();
            entity.Property(e => e.Weight).HasColumnName("weight").HasColumnType("decimal(10,3)");
            entity.Property(e => e.Volume).HasColumnName("volume").HasColumnType("decimal(10,3)");
            entity.Property(e => e.QuantityPerUnit).HasColumnName("quantity_per_unit").HasDefaultValue(1);
            
            // Đặt hàng
            entity.Property(e => e.MinOrderQuantity).HasColumnName("min_order_quantity").HasDefaultValue(1);
            entity.Property(e => e.MaxOrderQuantity).HasColumnName("max_order_quantity");
            
            // Hạn sử dụng và bảo quản
            entity.Property(e => e.ExpirationDate).HasColumnName("expiration_date");
            entity.Property(e => e.ShelfLifeDays).HasColumnName("shelf_life_days");
            entity.Property(e => e.StorageInstructions).HasColumnName("storage_instructions").HasMaxLength(500);
            entity.Property(e => e.IsPerishable).HasColumnName("is_perishable").HasDefaultValue(false);
            
            // Hình ảnh
            entity.Property(e => e.ImageUrl).HasColumnName("image_url").HasMaxLength(500);
            entity.Property(e => e.Images).HasColumnName("images");
            
            // Trạng thái
            entity.Property(e => e.IsAvailable).HasColumnName("is_available").IsRequired().HasDefaultValue(true);
            entity.Property(e => e.IsFeatured).HasColumnName("is_featured").HasDefaultValue(false);
            entity.Property(e => e.IsNew).HasColumnName("is_new").HasDefaultValue(false);
            entity.Property(e => e.IsOnSale).HasColumnName("is_on_sale").HasDefaultValue(false);
            entity.Property(e => e.IsDeleted).HasColumnName("is_deleted").IsRequired().HasDefaultValue(false);
            
            // SEO
            entity.Property(e => e.Slug).HasColumnName("slug").HasMaxLength(255);
            entity.Property(e => e.MetaTitle).HasColumnName("meta_title").HasMaxLength(255);
            entity.Property(e => e.MetaDescription).HasColumnName("meta_description").HasMaxLength(500);
            entity.Property(e => e.MetaKeywords).HasColumnName("meta_keywords").HasMaxLength(500);
            
            // Audit
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").IsRequired();
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.UpdatedBy).HasColumnName("updated_by");

            // Relationships
            entity.HasOne(e => e.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(e => e.CategoryId)
                .HasConstraintName("FK_products_categories");

            // Indexes
            entity.HasIndex(e => e.Sku).IsUnique().HasDatabaseName("IX_products_sku");
            entity.HasIndex(e => e.Barcode).IsUnique().HasDatabaseName("IX_products_barcode");
            entity.HasIndex(e => e.Name).HasDatabaseName("IX_products_name");
            entity.HasIndex(e => e.Slug).IsUnique().HasDatabaseName("IX_products_slug");
            entity.HasIndex(e => e.CategoryId).HasDatabaseName("IX_products_category_id");
            entity.HasIndex(e => e.Brand).HasDatabaseName("IX_products_brand");
            entity.HasIndex(e => e.IsAvailable).HasDatabaseName("IX_products_is_available");
            entity.HasIndex(e => e.IsFeatured).HasDatabaseName("IX_products_is_featured");
            entity.HasIndex(e => e.IsOnSale).HasDatabaseName("IX_products_is_on_sale");
            entity.HasIndex(e => e.Price).HasDatabaseName("IX_products_price");
            entity.HasIndex(e => e.CreatedAt).HasDatabaseName("IX_products_created_at");
            entity.HasIndex(e => e.IsDeleted).HasDatabaseName("IX_products_is_deleted");
        });
    }
}
