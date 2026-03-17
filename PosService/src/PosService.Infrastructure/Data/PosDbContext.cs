using Microsoft.EntityFrameworkCore;
using PosService.Domain.Entities;
using System.Reflection;

namespace PosService.Infrastructure.Data;

public class PosDbContext : DbContext
{
    public PosDbContext(DbContextOptions<PosDbContext> options) : base(options)
    {
    }

    public DbSet<Sale> Sales { get; set; }
    public DbSet<SaleItem> SaleItems { get; set; }
    public DbSet<Payment> Payments { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        // Configure Sale entity
        modelBuilder.Entity<Sale>(entity =>
        {
            entity.ToTable("sales");

            entity.Property(e => e.Id)
                .HasColumnName("id")
                .HasDefaultValueSql("NEWID()");

            entity.Property(e => e.SaleNumber)
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnName("sale_number");
            
            entity.HasIndex(e => e.SaleNumber, "IX_sales_sale_number").IsUnique();

            entity.Property(e => e.StoreId).HasColumnName("store_id");
            entity.Property(e => e.CashierId).HasColumnName("cashier_id");
            entity.Property(e => e.CustomerId).HasColumnName("customer_id");

            entity.Property(e => e.Subtotal)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("subtotal");

            entity.Property(e => e.DiscountAmount)
                .HasColumnType("decimal(18, 2)")
                .HasDefaultValue(0m)
                .HasColumnName("discount_amount");

            entity.Property(e => e.TaxAmount)
                .HasColumnType("decimal(18, 2)")
                .HasDefaultValue(0m)
                .HasColumnName("tax_amount");

            entity.Property(e => e.TotalAmount)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("total_amount");

            entity.Property(e => e.PaymentMethod)
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnName("payment_method");

            entity.Property(e => e.PaymentStatus)
                .IsRequired()
                .HasMaxLength(50)
                .HasDefaultValue("PENDING")
                .HasColumnName("payment_status");
            
            entity.Property(e => e.PaymentTransactionId).HasColumnName("payment_transaction_id");

            entity.Property(e => e.PromotionId).HasColumnName("promotion_id");
            entity.Property(e => e.VoucherCode).HasMaxLength(50).HasColumnName("voucher_code");
            
            entity.Property(e => e.PointsEarned).HasDefaultValue(0).HasColumnName("points_earned");
            entity.Property(e => e.PointsUsed).HasDefaultValue(0).HasColumnName("points_used");

            entity.Property(e => e.Status)
                .IsRequired()
                .HasMaxLength(50)
                .HasDefaultValue("COMPLETED")
                .HasColumnName("status");

            entity.Property(e => e.SaleDate)
                .HasDefaultValueSql("GETUTCDATE()")
                .HasColumnName("sale_date");
            
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()")
                .HasColumnName("created_at");

            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            
            entity.Property(e => e.Notes).HasColumnName("notes");

            entity.HasMany(e => e.SaleItems)
                  .WithOne(si => si.Sale)
                  .HasForeignKey(si => si.SaleId)
                  .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasMany(e => e.Payments)
                .WithOne(p => p.Sale)
                .HasForeignKey(p => p.SaleId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure SaleItem entity
        modelBuilder.Entity<SaleItem>(entity =>
        {
            entity.ToTable("sale_items");

            entity.Property(e => e.Id)
                .HasColumnName("id")
                .HasDefaultValueSql("NEWID()");

            entity.Property(e => e.SaleId).HasColumnName("sale_id");
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            
            entity.Property(e => e.ProductName)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("product_name");

            entity.Property(e => e.Sku)
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnName("sku");
            
            entity.Property(e => e.Barcode).HasMaxLength(50).HasColumnName("barcode");

            entity.Property(e => e.Quantity).HasColumnName("quantity");

            entity.Property(e => e.UnitPrice)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("unit_price");

            entity.Property(e => e.DiscountAmount)
                .HasColumnType("decimal(18, 2)")
                .HasDefaultValue(0m)
                .HasColumnName("discount_amount");

            entity.Property(e => e.LineTotal)
                .HasColumnType("decimal(18, 2)")
                .HasComputedColumnSql("(quantity * unit_price - discount_amount)", stored: true)
                .HasColumnName("line_total");
            
            entity.Property(e => e.PromotionApplied).HasDefaultValue(false).HasColumnName("promotion_applied");
            entity.Property(e => e.PromotionId).HasColumnName("promotion_id");
        });

        // Configure Payment entity
        modelBuilder.Entity<Payment>(entity =>
        {
            entity.ToTable("payments");

            entity.Property(e => e.Id)
                .HasColumnName("id")
                .HasDefaultValueSql("NEWID()");

            entity.Property(e => e.SaleId).HasColumnName("sale_id");

            entity.Property(e => e.PaymentMethod)
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnName("payment_method");

            entity.Property(e => e.Amount)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("amount");

            entity.Property(e => e.Status)
                .IsRequired()
                .HasMaxLength(50)
                .HasDefaultValue("PENDING")
                .HasColumnName("status");

            entity.Property(e => e.CashReceived)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("cash_received");

            entity.Property(e => e.CashChange)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("cash_change");

            entity.Property(e => e.TransactionReference)
                .HasMaxLength(255)
                .HasColumnName("transaction_reference");

            entity.Property(e => e.PaymentDate)
                .HasDefaultValueSql("GETUTCDATE()")
                .HasColumnName("payment_date");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()")
                .HasColumnName("created_at");
        });
    }
}
