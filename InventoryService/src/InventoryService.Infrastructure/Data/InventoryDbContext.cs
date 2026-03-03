using InventoryService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace InventoryService.Infrastructure.Data;

public class InventoryDbContext : DbContext
{
    public InventoryDbContext(DbContextOptions<InventoryDbContext> options) : base(options)
    {
    }

    // DbSets
    public DbSet<Warehouse> Warehouses { get; set; }
    public DbSet<WarehouseSlot> WarehouseSlots { get; set; }
    public DbSet<Inventory> Inventories { get; set; }
    public DbSet<ProductBatch> ProductBatches { get; set; }
    public DbSet<StockMovement> StockMovements { get; set; }
    public DbSet<StockMovementItem> StockMovementItems { get; set; }
    public DbSet<Transfer> Transfers { get; set; }
    public DbSet<TransferItem> TransferItems { get; set; }
    public DbSet<RestockRequest> RestockRequests { get; set; }
    public DbSet<RestockRequestItem> RestockRequestItems { get; set; }
    public DbSet<DamageReport> DamageReports { get; set; }
    public DbSet<InventoryCheck> InventoryChecks { get; set; }
    public DbSet<InventoryCheckItem> InventoryCheckItems { get; set; }
    public DbSet<InventoryHistory> InventoryHistory { get; set; }
    public DbSet<InventoryLog> InventoryLogs { get; set; }
    public DbSet<StoreReceivingLog> StoreReceivingLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // =====================================================
        // Warehouse Configuration
        // =====================================================
        modelBuilder.Entity<Warehouse>(entity =>
        {
            entity.ToTable("warehouses");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name).HasColumnName("name").IsRequired().HasMaxLength(255);
            entity.Property(e => e.Location).HasColumnName("location").HasMaxLength(500);
            entity.Property(e => e.Capacity).HasColumnName("capacity");
            entity.Property(e => e.Status).HasColumnName("status").IsRequired().HasMaxLength(50).HasDefaultValue("ACTIVE");
            entity.Property(e => e.IsDeleted).HasColumnName("is_deleted").HasDefaultValue(false);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");

            entity.HasIndex(e => e.Name).HasDatabaseName("IX_warehouses_name");
            entity.HasIndex(e => e.Status).HasDatabaseName("IX_warehouses_status");
        });

        // =====================================================
        // WarehouseSlot Configuration
        // =====================================================
        modelBuilder.Entity<WarehouseSlot>(entity =>
        {
            entity.ToTable("warehouse_slots");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.WarehouseId).HasColumnName("warehouse_id");
            entity.Property(e => e.SlotCode).HasColumnName("slot_code").IsRequired().HasMaxLength(50);
            entity.Property(e => e.Zone).HasColumnName("zone").HasMaxLength(10);
            entity.Property(e => e.RowNumber).HasColumnName("row_number");
            entity.Property(e => e.ColumnNumber).HasColumnName("column_number");
            entity.Property(e => e.Status).HasColumnName("status").IsRequired().HasMaxLength(50).HasDefaultValue("EMPTY");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("GETUTCDATE()");

            entity.HasOne(e => e.Warehouse)
                .WithMany(w => w.WarehouseSlots)
                .HasForeignKey(e => e.WarehouseId)
                .HasConstraintName("FK_warehouse_slots_warehouses");

            entity.HasIndex(e => e.WarehouseId).HasDatabaseName("IX_slots_warehouse_id");
            entity.HasIndex(e => e.Status).HasDatabaseName("IX_slots_status");
            entity.HasIndex(e => new { e.WarehouseId, e.SlotCode }).IsUnique().HasDatabaseName("UQ_warehouse_slot");
        });

        // =====================================================
        // Inventory Configuration
        // =====================================================
        modelBuilder.Entity<Inventory>(entity =>
        {
            entity.ToTable("inventories");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.LocationType).HasColumnName("location_type").IsRequired().HasMaxLength(50);
            entity.Property(e => e.LocationId).HasColumnName("location_id");
            entity.Property(e => e.Quantity).HasColumnName("quantity").HasDefaultValue(0);
            entity.Property(e => e.ReservedQuantity).HasColumnName("reserved_quantity").HasDefaultValue(0);
            entity.Property(e => e.MinStockLevel).HasColumnName("min_stock_level").HasDefaultValue(10);
            entity.Property(e => e.MaxStockLevel).HasColumnName("max_stock_level").HasDefaultValue(1000);
            entity.Property(e => e.LastStockCheck).HasColumnName("last_stock_check");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            entity.Ignore(e => e.AvailableQuantity); // Computed column

            entity.HasIndex(e => e.ProductId).HasDatabaseName("IX_inventories_product_id");
            entity.HasIndex(e => new { e.LocationType, e.LocationId }).HasDatabaseName("IX_inventories_location");
            entity.HasIndex(e => new { e.ProductId, e.LocationType, e.LocationId }).IsUnique().HasDatabaseName("UQ_inventory_product_location");
        });

        // =====================================================
        // ProductBatch Configuration
        // =====================================================
        modelBuilder.Entity<ProductBatch>(entity =>
        {
            entity.ToTable("product_batches");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.WarehouseId).HasColumnName("warehouse_id");
            entity.Property(e => e.SlotId).HasColumnName("slot_id");
            entity.Property(e => e.BatchNumber).HasColumnName("batch_number").IsRequired().HasMaxLength(100);
            entity.Property(e => e.Quantity).HasColumnName("quantity");
            entity.Property(e => e.ManufacturingDate).HasColumnName("manufacturing_date");
            entity.Property(e => e.ExpiryDate).HasColumnName("expiry_date");
            entity.Property(e => e.Supplier).HasColumnName("supplier").HasMaxLength(255);
            entity.Property(e => e.ReceivedAt).HasColumnName("received_at").HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.Status).HasColumnName("status").IsRequired().HasMaxLength(50).HasDefaultValue("AVAILABLE");

            entity.HasOne(e => e.Warehouse)
                .WithMany(w => w.ProductBatches)
                .HasForeignKey(e => e.WarehouseId)
                .HasConstraintName("FK_batches_warehouses");

            entity.HasOne(e => e.WarehouseSlot)
                .WithMany(s => s.ProductBatches)
                .HasForeignKey(e => e.SlotId)
                .HasConstraintName("FK_batches_slots");

            entity.HasIndex(e => e.ProductId).HasDatabaseName("IX_batches_product_id");
            entity.HasIndex(e => e.WarehouseId).HasDatabaseName("IX_batches_warehouse_id");
            entity.HasIndex(e => e.ExpiryDate).HasDatabaseName("IX_batches_expiry_date");
            entity.HasIndex(e => e.Status).HasDatabaseName("IX_batches_status");
            entity.HasIndex(e => e.BatchNumber).IsUnique().HasDatabaseName("UQ_batch_number");
        });

        // =====================================================
        // StockMovement Configuration
        // =====================================================
        modelBuilder.Entity<StockMovement>(entity =>
        {
            entity.ToTable("stock_movements");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.MovementNumber).HasColumnName("movement_number").IsRequired().HasMaxLength(50);
            entity.Property(e => e.MovementType).HasColumnName("movement_type").IsRequired().HasMaxLength(50);
            entity.Property(e => e.LocationId).HasColumnName("location_id");
            entity.Property(e => e.LocationType).HasColumnName("location_type").IsRequired().HasMaxLength(50);
            entity.Property(e => e.MovementDate).HasColumnName("movement_date").HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.Supplier).HasColumnName("supplier").HasMaxLength(255);
            entity.Property(e => e.PoNumber).HasColumnName("po_number").HasMaxLength(100);
            entity.Property(e => e.ReceivedBy).HasColumnName("received_by");
            entity.Property(e => e.Status).HasColumnName("status").IsRequired().HasMaxLength(50).HasDefaultValue("COMPLETED");
            entity.Property(e => e.Notes).HasColumnName("notes");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("GETUTCDATE()");

            entity.HasIndex(e => e.MovementNumber).IsUnique().HasDatabaseName("IX_movements_movement_number");
            entity.HasIndex(e => e.MovementType).HasDatabaseName("IX_movements_type");
            entity.HasIndex(e => new { e.LocationType, e.LocationId }).HasDatabaseName("IX_movements_location");
            entity.HasIndex(e => e.MovementDate).HasDatabaseName("IX_movements_date");
        });

        // =====================================================
        // StockMovementItem Configuration
        // =====================================================
        modelBuilder.Entity<StockMovementItem>(entity =>
        {
            entity.ToTable("stock_movement_items");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.MovementId).HasColumnName("movement_id");
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.BatchId).HasColumnName("batch_id");
            entity.Property(e => e.SlotId).HasColumnName("slot_id");
            entity.Property(e => e.Quantity).HasColumnName("quantity");
            entity.Property(e => e.UnitPrice).HasColumnName("unit_price").HasColumnType("decimal(18,2)");

            entity.HasOne(e => e.StockMovement)
                .WithMany(sm => sm.StockMovementItems)
                .HasForeignKey(e => e.MovementId)
                .HasConstraintName("FK_movement_items_movements");

            entity.HasOne(e => e.ProductBatch)
                .WithMany(b => b.StockMovementItems)
                .HasForeignKey(e => e.BatchId)
                .HasConstraintName("FK_movement_items_batches");

            entity.HasIndex(e => e.MovementId).HasDatabaseName("IX_movement_items_movement_id");
            entity.HasIndex(e => e.ProductId).HasDatabaseName("IX_movement_items_product_id");
        });

        // =====================================================
        // Transfer Configuration
        // =====================================================
        modelBuilder.Entity<Transfer>(entity =>
        {
            entity.ToTable("transfers");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.TransferNumber).HasColumnName("transfer_number").IsRequired().HasMaxLength(50);
            entity.Property(e => e.FromLocationType).HasColumnName("from_location_type").IsRequired().HasMaxLength(50);
            entity.Property(e => e.FromLocationId).HasColumnName("from_location_id");
            entity.Property(e => e.ToLocationType).HasColumnName("to_location_type").IsRequired().HasMaxLength(50);
            entity.Property(e => e.ToLocationId).HasColumnName("to_location_id");
            entity.Property(e => e.TransferDate).HasColumnName("transfer_date").HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.ExpectedDelivery).HasColumnName("expected_delivery");
            entity.Property(e => e.ActualDelivery).HasColumnName("actual_delivery");
            entity.Property(e => e.Status).HasColumnName("status").IsRequired().HasMaxLength(50).HasDefaultValue("PENDING");
            entity.Property(e => e.ShippedBy).HasColumnName("shipped_by");
            entity.Property(e => e.ReceivedBy).HasColumnName("received_by");
            entity.Property(e => e.Notes).HasColumnName("notes");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

            entity.HasIndex(e => e.TransferNumber).IsUnique().HasDatabaseName("IX_transfers_transfer_number");
            entity.HasIndex(e => new { e.FromLocationType, e.FromLocationId }).HasDatabaseName("IX_transfers_from_location");
            entity.HasIndex(e => new { e.ToLocationType, e.ToLocationId }).HasDatabaseName("IX_transfers_to_location");
            entity.HasIndex(e => e.Status).HasDatabaseName("IX_transfers_status");
            entity.HasIndex(e => e.TransferDate).HasDatabaseName("IX_transfers_date");
        });

        // =====================================================
        // TransferItem Configuration
        // =====================================================
        modelBuilder.Entity<TransferItem>(entity =>
        {
            entity.ToTable("transfer_items");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.TransferId).HasColumnName("transfer_id");
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.BatchId).HasColumnName("batch_id");
            entity.Property(e => e.RequestedQuantity).HasColumnName("requested_quantity");
            entity.Property(e => e.ShippedQuantity).HasColumnName("shipped_quantity");
            entity.Property(e => e.ReceivedQuantity).HasColumnName("received_quantity");
            entity.Property(e => e.DamagedQuantity).HasColumnName("damaged_quantity").HasDefaultValue(0);
            entity.Property(e => e.Notes).HasColumnName("notes").HasMaxLength(500);

            entity.HasOne(e => e.Transfer)
                .WithMany(t => t.TransferItems)
                .HasForeignKey(e => e.TransferId)
                .HasConstraintName("FK_transfer_items_transfers");

            entity.HasOne(e => e.ProductBatch)
                .WithMany(b => b.TransferItems)
                .HasForeignKey(e => e.BatchId)
                .HasConstraintName("FK_transfer_items_batches");

            entity.HasIndex(e => e.TransferId).HasDatabaseName("IX_transfer_items_transfer_id");
            entity.HasIndex(e => e.ProductId).HasDatabaseName("IX_transfer_items_product_id");
        });

        // =====================================================
        // RestockRequest Configuration
        // =====================================================
        modelBuilder.Entity<RestockRequest>(entity =>
        {
            entity.ToTable("restock_requests");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.RequestNumber).HasColumnName("request_number").IsRequired().HasMaxLength(50);
            entity.Property(e => e.StoreId).HasColumnName("store_id");
            entity.Property(e => e.WarehouseId).HasColumnName("warehouse_id");
            entity.Property(e => e.RequestedBy).HasColumnName("requested_by");
            entity.Property(e => e.RequestedDate).HasColumnName("requested_date").HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.Priority).HasColumnName("priority").IsRequired().HasMaxLength(50).HasDefaultValue("NORMAL");
            entity.Property(e => e.Status).HasColumnName("status").IsRequired().HasMaxLength(50).HasDefaultValue("PENDING");
            entity.Property(e => e.ApprovedBy).HasColumnName("approved_by");
            entity.Property(e => e.ApprovedDate).HasColumnName("approved_date");
            entity.Property(e => e.TransferId).HasColumnName("transfer_id");
            entity.Property(e => e.Notes).HasColumnName("notes");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

            entity.HasOne(e => e.Transfer)
                .WithMany(t => t.RestockRequests)
                .HasForeignKey(e => e.TransferId)
                .HasConstraintName("FK_restock_transfers");

            entity.HasIndex(e => e.RequestNumber).IsUnique().HasDatabaseName("IX_restock_request_number");
            entity.HasIndex(e => e.StoreId).HasDatabaseName("IX_restock_store_id");
            entity.HasIndex(e => e.Status).HasDatabaseName("IX_restock_status");
            entity.HasIndex(e => e.Priority).HasDatabaseName("IX_restock_priority");
        });

        // =====================================================
        // RestockRequestItem Configuration
        // =====================================================
        modelBuilder.Entity<RestockRequestItem>(entity =>
        {
            entity.ToTable("restock_request_items");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.RequestId).HasColumnName("request_id");
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.RequestedQuantity).HasColumnName("requested_quantity");
            entity.Property(e => e.CurrentQuantity).HasColumnName("current_quantity");
            entity.Property(e => e.ApprovedQuantity).HasColumnName("approved_quantity");
            entity.Property(e => e.Reason).HasColumnName("reason").HasMaxLength(500);

            entity.HasOne(e => e.RestockRequest)
                .WithMany(r => r.RestockRequestItems)
                .HasForeignKey(e => e.RequestId)
                .HasConstraintName("FK_restock_items_requests");

            entity.HasIndex(e => e.RequestId).HasDatabaseName("IX_restock_items_request_id");
            entity.HasIndex(e => e.ProductId).HasDatabaseName("IX_restock_items_product_id");
        });

        // =====================================================
        // DamageReport Configuration
        // =====================================================
        modelBuilder.Entity<DamageReport>(entity =>
        {
            entity.ToTable("damage_reports");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ReportNumber).HasColumnName("report_number").IsRequired().HasMaxLength(50);
            entity.Property(e => e.LocationType).HasColumnName("location_type").IsRequired().HasMaxLength(50);
            entity.Property(e => e.LocationId).HasColumnName("location_id");
            entity.Property(e => e.DamageType).HasColumnName("damage_type").IsRequired().HasMaxLength(50);
            entity.Property(e => e.ReportedBy).HasColumnName("reported_by");
            entity.Property(e => e.ReportedDate).HasColumnName("reported_date").HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.TotalValue).HasColumnName("total_value").HasColumnType("decimal(18,2)");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Photos).HasColumnName("photos");
            entity.Property(e => e.Status).HasColumnName("status").IsRequired().HasMaxLength(50).HasDefaultValue("PENDING");
            entity.Property(e => e.ApprovedBy).HasColumnName("approved_by");
            entity.Property(e => e.ApprovedDate).HasColumnName("approved_date");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("GETUTCDATE()");

            entity.HasIndex(e => e.ReportNumber).IsUnique().HasDatabaseName("IX_damage_report_number");
            entity.HasIndex(e => new { e.LocationType, e.LocationId }).HasDatabaseName("IX_damage_location");
            entity.HasIndex(e => e.DamageType).HasDatabaseName("IX_damage_type");
            entity.HasIndex(e => e.Status).HasDatabaseName("IX_damage_status");
        });

        // =====================================================
        // InventoryCheck Configuration
        // =====================================================
        modelBuilder.Entity<InventoryCheck>(entity =>
        {
            entity.ToTable("inventory_checks");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CheckNumber).HasColumnName("check_number").IsRequired().HasMaxLength(50);
            entity.Property(e => e.LocationType).HasColumnName("location_type").IsRequired().HasMaxLength(50);
            entity.Property(e => e.LocationId).HasColumnName("location_id");
            entity.Property(e => e.CheckType).HasColumnName("check_type").IsRequired().HasMaxLength(50);
            entity.Property(e => e.CheckDate).HasColumnName("check_date").HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.CheckedBy).HasColumnName("checked_by");
            entity.Property(e => e.Status).HasColumnName("status").IsRequired().HasMaxLength(50).HasDefaultValue("PENDING");
            entity.Property(e => e.TotalDiscrepancies).HasColumnName("total_discrepancies").HasDefaultValue(0);
            entity.Property(e => e.Notes).HasColumnName("notes");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("GETUTCDATE()");

            entity.HasIndex(e => e.CheckNumber).IsUnique().HasDatabaseName("IX_check_number");
            entity.HasIndex(e => new { e.LocationType, e.LocationId }).HasDatabaseName("IX_check_location");
            entity.HasIndex(e => e.Status).HasDatabaseName("IX_check_status");
        });

        // =====================================================
        // InventoryCheckItem Configuration
        // =====================================================
        modelBuilder.Entity<InventoryCheckItem>(entity =>
        {
            entity.ToTable("inventory_check_items");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CheckId).HasColumnName("check_id");
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.SystemQuantity).HasColumnName("system_quantity");
            entity.Property(e => e.ActualQuantity).HasColumnName("actual_quantity");
            entity.Property(e => e.Note).HasColumnName("note").HasMaxLength(500);
            entity.Ignore(e => e.Difference); // Computed column

            entity.HasOne(e => e.InventoryCheck)
                .WithMany(c => c.InventoryCheckItems)
                .HasForeignKey(e => e.CheckId)
                .HasConstraintName("FK_check_items_checks");

            entity.HasIndex(e => e.CheckId).HasDatabaseName("IX_check_items_check_id");
            entity.HasIndex(e => e.ProductId).HasDatabaseName("IX_check_items_product_id");
        });

        // =====================================================
        // InventoryHistory Configuration
        // =====================================================
        modelBuilder.Entity<InventoryHistory>(entity =>
        {
            entity.ToTable("inventory_history");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(e => e.InventoryId).HasColumnName("inventory_id");
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.LocationType).HasColumnName("location_type").IsRequired().HasMaxLength(50);
            entity.Property(e => e.LocationId).HasColumnName("location_id");
            entity.Property(e => e.SnapshotDate).HasColumnName("snapshot_date");
            entity.Property(e => e.Quantity).HasColumnName("quantity");
            entity.Property(e => e.ReservedQuantity).HasColumnName("reserved_quantity");
            entity.Property(e => e.AvailableQuantity).HasColumnName("available_quantity");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("GETUTCDATE()");

            entity.HasIndex(e => e.InventoryId).HasDatabaseName("IX_history_inventory_id");
            entity.HasIndex(e => e.SnapshotDate).HasDatabaseName("IX_history_snapshot_date");
        });

        // =====================================================
        // InventoryLog Configuration
        // =====================================================
        modelBuilder.Entity<InventoryLog>(entity =>
        {
            entity.ToTable("inventory_logs");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(e => e.InventoryId).HasColumnName("inventory_id");
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.Action).HasColumnName("action").IsRequired().HasMaxLength(100);
            entity.Property(e => e.OldQuantity).HasColumnName("old_quantity");
            entity.Property(e => e.NewQuantity).HasColumnName("new_quantity");
            entity.Property(e => e.Reason).HasColumnName("reason").HasMaxLength(500);
            entity.Property(e => e.PerformedBy).HasColumnName("performed_by");
            entity.Property(e => e.PerformedAt).HasColumnName("performed_at").HasDefaultValueSql("GETUTCDATE()");
            entity.Ignore(e => e.QuantityChange); // Computed column

            entity.HasIndex(e => e.InventoryId).HasDatabaseName("IX_logs_inventory_id");
            entity.HasIndex(e => e.ProductId).HasDatabaseName("IX_logs_product_id");
            entity.HasIndex(e => e.Action).HasDatabaseName("IX_logs_action");
            entity.HasIndex(e => e.PerformedAt).HasDatabaseName("IX_logs_performed_at");
        });

        // =====================================================
        // StoreReceivingLog Configuration
        // =====================================================
        modelBuilder.Entity<StoreReceivingLog>(entity =>
        {
            entity.ToTable("store_receiving_logs");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.TransferId).HasColumnName("transfer_id");
            entity.Property(e => e.StoreId).HasColumnName("store_id");
            entity.Property(e => e.ReceivedBy).HasColumnName("received_by");
            entity.Property(e => e.ReceivedDate).HasColumnName("received_date").HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.ConditionStatus).HasColumnName("condition_status").IsRequired().HasMaxLength(50);
            entity.Property(e => e.Notes).HasColumnName("notes");
            entity.Property(e => e.Photos).HasColumnName("photos");

            entity.HasOne(e => e.Transfer)
                .WithMany(t => t.StoreReceivingLogs)
                .HasForeignKey(e => e.TransferId)
                .HasConstraintName("FK_receiving_transfers");

            entity.HasIndex(e => e.TransferId).HasDatabaseName("IX_receiving_transfer_id");
            entity.HasIndex(e => e.StoreId).HasDatabaseName("IX_receiving_store_id");
        });
    }
}
