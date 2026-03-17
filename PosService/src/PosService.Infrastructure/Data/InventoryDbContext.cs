using Microsoft.EntityFrameworkCore;

namespace PosService.Infrastructure.Data;

/// <summary>
/// Read-only DbContext for InventoryDB
/// Used by POS service to query stock information without modifications
/// </summary>
public class InventoryDbContext : DbContext
{
    public InventoryDbContext(DbContextOptions<InventoryDbContext> options) : base(options)
    {
    }

    public DbSet<InventoryEntity> Inventories { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<InventoryEntity>(entity =>
        {
            entity.ToTable("inventories");
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.LocationType).HasColumnName("location_type").HasMaxLength(50);
            entity.Property(e => e.LocationId).HasColumnName("location_id");
            entity.Property(e => e.Quantity).HasColumnName("quantity");
            entity.Property(e => e.ReservedQuantity).HasColumnName("reserved_quantity");
            
            // Indexes
            entity.HasIndex(e => e.ProductId).HasDatabaseName("IX_inventories_product_id");
        });
    }
}

/// <summary>
/// Inventory entity for read-only access
/// </summary>
public class InventoryEntity
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string LocationType { get; set; } = string.Empty;
    public Guid LocationId { get; set; }
    public int Quantity { get; set; }
    public int ReservedQuantity { get; set; }
    
    /// <summary>
    /// Computed property: Quantity - ReservedQuantity
    /// </summary>
    public int AvailableQuantity => Quantity - ReservedQuantity;
}
