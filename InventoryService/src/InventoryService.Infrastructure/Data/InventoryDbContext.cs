using InventoryService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace InventoryService.Infrastructure.Data;

public class InventoryDbContext : DbContext
{
    public InventoryDbContext(DbContextOptions<InventoryDbContext> options) : base(options)
    {
    }

    public DbSet<Warehouse> Warehouses { get; set; }
    public DbSet<WarehouseSlot> WarehouseSlots { get; set; }
    public DbSet<Inventory> Inventories { get; set; }
    public DbSet<ProductBatch> ProductBatches { get; set; }
    public DbSet<StockMovement> StockMovements { get; set; }
    public DbSet<StockMovementItem> StockMovementItems { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Warehouse Configuration
        modelBuilder.Entity<Warehouse>(entity =>
        {
            entity.ToTable("warehouses");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name).HasColumnName("name").IsRequired().HasMaxLength(255);
            entity.Property(e => e.Location).HasColumnName("location").HasMaxLength(500);
            entity.Property(e => e.IsDeleted).HasColumnName("is_deleted").HasDefaultValue(false);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.UpdatedBy).HasColumnName("updated_by");

            entity.HasIndex(e => e.Name).HasDatabaseName("IX_warehouses_name");
            entity.HasIndex(e => e.IsDeleted).HasDatabaseName("IX_warehouses_is_deleted");
        });

        // WarehouseSlot Configuration
        modelBuilder.Entity<WarehouseSlot>(entity =>
        {
            entity.ToTable("warehouse_slots");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.WarehouseId).HasColumnName("warehouse_id");
            entity.Property(e => e.SlotCode).HasColumnName("slot_code").IsRequired().HasMaxLength(50);
            entity.Property(e => e.Capacity).HasColumnName("capacity").HasDefaultValue(0);
            entity.Property(e => e.Status).HasColumnName("status").IsRequired().HasMaxLength(20).HasDefaultValue("AVAILABLE");
            entity.Property(e => e.IsDeleted).HasColumnName("is_deleted").HasDefaultValue(false);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.UpdatedBy).HasColumnName("updated_by");

            entity.HasOne(e => e.Warehouse)
                .WithMany(w => w.WarehouseSlots)
                .HasForeignKey(e => e.WarehouseId)
                .HasConstraintName("FK_warehouse_slots_warehouses");

            entity.HasIndex(e => new { e.WarehouseId, e.SlotCode })
                .IsUnique()
                .HasDatabaseName("UX_warehouse_slots_code");
            entity.HasIndex(e => e.Status).HasDatabaseName("IX_warehouse_slots_status");
            entity.HasIndex(e => e.IsDeleted).HasDatabaseName("IX_warehouse_slots_is_deleted");
        });

        // Inventory Configuration
        modelBuilder.Entity<Inventory>(entity =>
        {
            entity.ToTable("inventories");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.StoreId).HasColumnName("store_id");
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.Quantity).HasColumnName("quantity").HasDefaultValue(0);
            entity.Property(e => e.AlertThreshold).HasColumnName("alert_threshold").HasDefaultValue(10);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.UpdatedBy).HasColumnName("updated_by");

            entity.HasIndex(e => e.StoreId).HasDatabaseName("IX_inventories_store_id");
            entity.HasIndex(e => e.ProductId).HasDatabaseName("IX_inventories_product_id");
            entity.HasIndex(e => e.Quantity).HasDatabaseName("IX_inventories_quantity");
            entity.HasIndex(e => new { e.StoreId, e.ProductId })
                .IsUnique()
                .HasDatabaseName("UX_inventories_store_product");
        });

        // ProductBatch Configuration
        modelBuilder.Entity<ProductBatch>(entity =>
        {
            entity.ToTable("product_batches");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.WarehouseId).HasColumnName("warehouse_id");
            entity.Property(e => e.BatchCode).HasColumnName("batch_code").IsRequired().HasMaxLength(100);
            entity.Property(e => e.ManufactureDate).HasColumnName("manufacture_date");
            entity.Property(e => e.ExpirationDate).HasColumnName("expiration_date");
            entity.Property(e => e.Quantity).HasColumnName("quantity").HasDefaultValue(0);
            entity.Property(e => e.IsDeleted).HasColumnName("is_deleted").HasDefaultValue(false);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.UpdatedBy).HasColumnName("updated_by");

            entity.HasOne(e => e.Warehouse)
                .WithMany(w => w.ProductBatches)
                .HasForeignKey(e => e.WarehouseId)
                .HasConstraintName("FK_product_batches_warehouses");

            entity.HasIndex(e => e.ProductId).HasDatabaseName("IX_product_batches_product_id");
            entity.HasIndex(e => e.WarehouseId).HasDatabaseName("IX_product_batches_warehouse_id");
            entity.HasIndex(e => e.BatchCode).HasDatabaseName("IX_product_batches_batch_code");
            entity.HasIndex(e => e.ExpirationDate).HasDatabaseName("IX_product_batches_expiration_date");
            entity.HasIndex(e => e.IsDeleted).HasDatabaseName("IX_product_batches_is_deleted");
        });

        // StockMovement Configuration
        modelBuilder.Entity<StockMovement>(entity =>
        {
            entity.ToTable("stock_movements");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.WarehouseId).HasColumnName("warehouse_id");
            entity.Property(e => e.StoreId).HasColumnName("store_id");
            entity.Property(e => e.MovementType).HasColumnName("movement_type").IsRequired().HasMaxLength(50);
            entity.Property(e => e.SourceInfo).HasColumnName("source_info").HasMaxLength(500);
            entity.Property(e => e.Note).HasColumnName("note");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("GETUTCDATE()");

            entity.HasOne(e => e.Warehouse)
                .WithMany(w => w.StockMovements)
                .HasForeignKey(e => e.WarehouseId)
                .HasConstraintName("FK_stock_movements_warehouses");

            entity.HasIndex(e => e.WarehouseId).HasDatabaseName("IX_stock_movements_warehouse_id");
            entity.HasIndex(e => e.StoreId).HasDatabaseName("IX_stock_movements_store_id");
            entity.HasIndex(e => e.MovementType).HasDatabaseName("IX_stock_movements_movement_type");
            entity.HasIndex(e => e.CreatedAt).HasDatabaseName("IX_stock_movements_created_at");
            entity.HasIndex(e => e.CreatedBy).HasDatabaseName("IX_stock_movements_created_by");
        });

        // StockMovementItem Configuration
        modelBuilder.Entity<StockMovementItem>(entity =>
        {
            entity.ToTable("stock_movement_items");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.MovementId).HasColumnName("movement_id");
            entity.Property(e => e.BatchId).HasColumnName("batch_id");
            entity.Property(e => e.Quantity).HasColumnName("quantity");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("GETUTCDATE()");

            entity.HasOne(e => e.StockMovement)
                .WithMany(sm => sm.StockMovementItems)
                .HasForeignKey(e => e.MovementId)
                .HasConstraintName("FK_stock_movement_items_movements");

            entity.HasOne(e => e.ProductBatch)
                .WithMany()
                .HasForeignKey(e => e.BatchId)
                .HasConstraintName("FK_stock_movement_items_batches");

            entity.HasIndex(e => e.MovementId).HasDatabaseName("IX_stock_movement_items_movement_id");
            entity.HasIndex(e => e.BatchId).HasDatabaseName("IX_stock_movement_items_batch_id");
        });
    }
}
