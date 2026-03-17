using Microsoft.EntityFrameworkCore;

namespace PosService.Infrastructure.Data;

/// <summary>
/// Read-only DbContext for ProductDB
/// Used by POS service to query product catalog without modifications
/// </summary>
public class ProductDbContext : DbContext
{
    public ProductDbContext(DbContextOptions<ProductDbContext> options) : base(options)
    {
    }

    public DbSet<ProductEntity> Products { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ProductEntity>(entity =>
        {
            entity.ToTable("products");
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Sku).HasColumnName("sku").HasMaxLength(100);
            entity.Property(e => e.Barcode).HasColumnName("barcode").HasMaxLength(100);
            entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(500);
            entity.Property(e => e.Price).HasColumnName("price").HasColumnType("decimal(18,2)");
            entity.Property(e => e.OriginalPrice).HasColumnName("original_price").HasColumnType("decimal(18,2)");
            entity.Property(e => e.Brand).HasColumnName("brand").HasMaxLength(255);
            entity.Property(e => e.Unit).HasColumnName("unit").HasMaxLength(50);
            entity.Property(e => e.ImageUrl).HasColumnName("image_url").HasMaxLength(500);
            entity.Property(e => e.IsAvailable).HasColumnName("is_available");
            entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");
            entity.Property(e => e.IsOnSale).HasColumnName("is_on_sale");

            // Indexes
            entity.HasIndex(e => e.Barcode).HasDatabaseName("IX_products_barcode");
            entity.HasIndex(e => e.Sku).HasDatabaseName("IX_products_sku");
            entity.HasIndex(e => e.Name).HasDatabaseName("IX_products_name");
        });
    }
}

/// <summary>
/// Product entity for read-only access
/// </summary>
public class ProductEntity
{
    public Guid Id { get; set; }
    public string Sku { get; set; } = string.Empty;
    public string? Barcode { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public decimal? OriginalPrice { get; set; }
    public string? Brand { get; set; }
    public string Unit { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public bool IsAvailable { get; set; }
    public bool IsDeleted { get; set; }
    public bool IsOnSale { get; set; }
}
