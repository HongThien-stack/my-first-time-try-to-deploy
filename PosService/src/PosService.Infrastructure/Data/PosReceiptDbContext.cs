using Microsoft.EntityFrameworkCore;

namespace PosService.Infrastructure.Data;

public class PosReceiptDbContext : DbContext
{
    public PosReceiptDbContext(DbContextOptions<PosReceiptDbContext> options) : base(options)
    {
    }

    public DbSet<SaleReadEntity> Sales => Set<SaleReadEntity>();
    public DbSet<SaleItemReadEntity> SaleItems => Set<SaleItemReadEntity>();
    public DbSet<PaymentReadEntity> Payments => Set<PaymentReadEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<SaleReadEntity>(entity =>
        {
            entity.ToTable("sales");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.SaleNumber).HasColumnName("sale_number").HasMaxLength(50);
            entity.Property(x => x.StoreId).HasColumnName("store_id");
            entity.Property(x => x.CashierId).HasColumnName("cashier_id");
            entity.Property(x => x.Subtotal).HasColumnName("subtotal").HasColumnType("decimal(18,2)");
            entity.Property(x => x.DiscountAmount).HasColumnName("discount_amount").HasColumnType("decimal(18,2)");
            entity.Property(x => x.TaxAmount).HasColumnName("tax_amount").HasColumnType("decimal(18,2)");
            entity.Property(x => x.TotalAmount).HasColumnName("total_amount").HasColumnType("decimal(18,2)");
            entity.Property(x => x.PaymentMethod).HasColumnName("payment_method").HasMaxLength(50);
            entity.Property(x => x.PaymentStatus).HasColumnName("payment_status").HasMaxLength(50);
            entity.Property(x => x.SaleDate).HasColumnName("sale_date");
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
        });

        modelBuilder.Entity<SaleItemReadEntity>(entity =>
        {
            entity.ToTable("sale_items");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.SaleId).HasColumnName("sale_id");
            entity.Property(x => x.ProductName).HasColumnName("product_name").HasMaxLength(255);
            entity.Property(x => x.Sku).HasColumnName("sku").HasMaxLength(50);
            entity.Property(x => x.Quantity).HasColumnName("quantity");
            entity.Property(x => x.UnitPrice).HasColumnName("unit_price").HasColumnType("decimal(18,2)");
            entity.Property(x => x.DiscountAmount).HasColumnName("discount_amount").HasColumnType("decimal(18,2)");
            entity.Property(x => x.LineTotal).HasColumnName("line_total").HasColumnType("decimal(29,2)");
        });

        modelBuilder.Entity<PaymentReadEntity>(entity =>
        {
            entity.ToTable("payments");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.SaleId).HasColumnName("sale_id");
            entity.Property(x => x.PaymentMethod).HasColumnName("payment_method").HasMaxLength(50);
            entity.Property(x => x.Amount).HasColumnName("amount").HasColumnType("decimal(18,2)");
            entity.Property(x => x.Status).HasColumnName("status").HasMaxLength(50);
            entity.Property(x => x.CashReceived).HasColumnName("cash_received").HasColumnType("decimal(18,2)");
            entity.Property(x => x.CashChange).HasColumnName("cash_change").HasColumnType("decimal(18,2)");
            entity.Property(x => x.TransactionReference).HasColumnName("transaction_reference").HasMaxLength(255);
            entity.Property(x => x.PaymentDate).HasColumnName("payment_date");
        });
    }
}

public class SaleReadEntity
{
    public Guid Id { get; set; }
    public string SaleNumber { get; set; } = string.Empty;
    public Guid StoreId { get; set; }
    public Guid CashierId { get; set; }
    public decimal Subtotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string PaymentStatus { get; set; } = string.Empty;
    public DateTime SaleDate { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class SaleItemReadEntity
{
    public Guid Id { get; set; }
    public Guid SaleId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal LineTotal { get; set; }
}

public class PaymentReadEntity
{
    public Guid Id { get; set; }
    public Guid SaleId { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal? CashReceived { get; set; }
    public decimal? CashChange { get; set; }
    public string? TransactionReference { get; set; }
    public DateTime PaymentDate { get; set; }
}
