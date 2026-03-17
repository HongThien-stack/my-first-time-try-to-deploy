using Microsoft.EntityFrameworkCore;
using PromotionService.Application.Interfaces;
using PromotionService.Domain.Entities;

namespace PromotionService.Infrastructure.Persistence
{
    public class PromotionDbContext : DbContext, IPromotionDbContext
    {
        public PromotionDbContext(DbContextOptions<PromotionDbContext> options) : base(options)
        {
        }

        public DbSet<Promotion> Promotions => Set<Promotion>();
        public DbSet<PromotionRule> PromotionRules => Set<PromotionRule>();
        public DbSet<Voucher> Vouchers => Set<Voucher>();
        public DbSet<MembershipTier> MembershipTiers => Set<MembershipTier>();
        public DbSet<CustomerLoyalty> CustomerLoyalties => Set<CustomerLoyalty>();
        public DbSet<PointsTransaction> PointsTransactions => Set<PointsTransaction>();
        public DbSet<Reward> Rewards => Set<Reward>();
        public DbSet<RewardRedemption> RewardRedemptions => Set<RewardRedemption>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Promotion entity
            modelBuilder.Entity<Promotion>(entity =>
            {
                entity.ToTable("promotions");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.PromotionCode).HasColumnName("promotion_code").IsRequired();
                entity.Property(e => e.Name).HasColumnName("name").IsRequired();
                entity.Property(e => e.Description).HasColumnName("description");
                entity.Property(e => e.PromotionType).HasColumnName("promotion_type").IsRequired();
                entity.Property(e => e.DiscountPercentage).HasColumnName("discount_percentage").HasColumnType("decimal(5,2)");
                entity.Property(e => e.DiscountAmount).HasColumnName("discount_amount").HasColumnType("decimal(18,2)");
                entity.Property(e => e.MinPurchaseAmount).HasColumnName("min_purchase_amount").HasColumnType("decimal(18,2)");
                entity.Property(e => e.MaxDiscountAmount).HasColumnName("max_discount_amount").HasColumnType("decimal(18,2)");
                entity.Property(e => e.ApplicableTo).HasColumnName("applicable_to");
                entity.Property(e => e.StartDate).HasColumnName("start_date");
                entity.Property(e => e.EndDate).HasColumnName("end_date");
                entity.Property(e => e.UsageLimit).HasColumnName("usage_limit");
                entity.Property(e => e.UsageLimitPerCustomer).HasColumnName("usage_limit_per_customer");
                entity.Property(e => e.UsageCount).HasColumnName("usage_count");
                entity.Property(e => e.IsActive).HasColumnName("is_active");
                entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");
                entity.Property(e => e.CreatedBy).HasColumnName("created_by");
                entity.Property(e => e.CreatedAt).HasColumnName("created_at");
                entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
                entity.HasMany(e => e.Rules).WithOne(r => r.Promotion).HasForeignKey(r => r.PromotionId).OnDelete(DeleteBehavior.Cascade);
            });
            
            // PromotionRule entity
            modelBuilder.Entity<PromotionRule>(entity =>
            {
                entity.ToTable("promotion_rules");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.PromotionId).HasColumnName("promotion_id");
                entity.Property(e => e.RuleType).HasColumnName("rule_type");
                entity.Property(e => e.RuleCondition).HasColumnName("rule_condition");
                entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            });
            
            // Voucher entity
            modelBuilder.Entity<Voucher>(entity =>
            {
                entity.ToTable("vouchers");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.VoucherCode).HasColumnName("voucher_code");
                entity.Property(e => e.PromotionId).HasColumnName("promotion_id");
                entity.Property(e => e.CustomerId).HasColumnName("customer_id");
                entity.Property(e => e.IsUsed).HasColumnName("is_used");
                entity.Property(e => e.UsedAt).HasColumnName("used_at");
                entity.Property(e => e.UsedInSaleId).HasColumnName("used_in_sale_id");
                entity.Property(e => e.ExpiresAt).HasColumnName("expires_at");
                entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            });
            
            // MembershipTier entity
            modelBuilder.Entity<MembershipTier>(entity =>
            {
                entity.ToTable("membership_tiers");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.TierName).HasColumnName("tier_name");
                entity.Property(e => e.TierLevel).HasColumnName("tier_level");
                entity.Property(e => e.MinPoints).HasColumnName("min_points");
                entity.Property(e => e.MinPurchases).HasColumnName("min_purchases").HasColumnType("decimal(18,2)");
                entity.Property(e => e.DiscountPercentage).HasColumnName("discount_percentage").HasColumnType("decimal(5,2)");
                entity.Property(e => e.PointsMultiplier).HasColumnName("points_multiplier").HasColumnType("decimal(4,2)");
                entity.Property(e => e.BirthdayBonusPoints).HasColumnName("birthday_bonus_points");
                entity.Property(e => e.Color).HasColumnName("color");
                entity.Property(e => e.IconUrl).HasColumnName("icon_url");
                entity.Property(e => e.IsActive).HasColumnName("is_active");
                entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            });
            
            // CustomerLoyalty entity
            modelBuilder.Entity<CustomerLoyalty>(entity =>
            {
                entity.ToTable("customer_loyalty");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.CustomerId).HasColumnName("customer_id");
                entity.Property(e => e.MembershipTierId).HasColumnName("membership_tier_id");
                entity.Property(e => e.TotalPoints).HasColumnName("total_points");
                entity.Property(e => e.AvailablePoints).HasColumnName("available_points");
                entity.Property(e => e.UsedPoints).HasColumnName("used_points");
                entity.Property(e => e.ExpiredPoints).HasColumnName("expired_points");
                entity.Property(e => e.TotalPurchases).HasColumnName("total_purchases").HasColumnType("decimal(18,2)");
                entity.Property(e => e.PurchaseCount).HasColumnName("purchase_count");
                entity.Property(e => e.JoinedAt).HasColumnName("joined_at");
                entity.Property(e => e.LastPurchaseAt).HasColumnName("last_purchase_at");
                entity.Property(e => e.TierUpgradedAt).HasColumnName("tier_upgraded_at");
                entity.HasOne(e => e.MembershipTier).WithMany().HasForeignKey(e => e.MembershipTierId);
            });
            
            // PointsTransaction entity
            modelBuilder.Entity<PointsTransaction>(entity =>
            {
                entity.ToTable("points_transactions");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.CustomerId).HasColumnName("customer_id");
                entity.Property(e => e.TransactionType).HasColumnName("transaction_type");
                entity.Property(e => e.Points).HasColumnName("points");
                entity.Property(e => e.SaleId).HasColumnName("sale_id");
                entity.Property(e => e.RedemptionId).HasColumnName("redemption_id");
                entity.Property(e => e.BalanceBefore).HasColumnName("balance_before");
                entity.Property(e => e.BalanceAfter).HasColumnName("balance_after");
                entity.Property(e => e.Description).HasColumnName("description");
                entity.Property(e => e.ExpiresAt).HasColumnName("expires_at");
                entity.Property(e => e.CreatedAt).HasColumnName("created_at");
                entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            });
            
            // Reward entity
            modelBuilder.Entity<Reward>(entity =>
            {
                entity.ToTable("rewards_catalog");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.RewardCode).HasColumnName("reward_code");
                entity.Property(e => e.RewardName).HasColumnName("reward_name");
                entity.Property(e => e.Description).HasColumnName("description");
                entity.Property(e => e.RewardType).HasColumnName("reward_type");
                entity.Property(e => e.PointsCost).HasColumnName("points_cost");
                entity.Property(e => e.DiscountAmount).HasColumnName("discount_amount").HasColumnType("decimal(18,2)");
                entity.Property(e => e.ProductId).HasColumnName("product_id");
                entity.Property(e => e.StockQuantity).HasColumnName("stock_quantity");
                entity.Property(e => e.RedeemedCount).HasColumnName("redeemed_count");
                entity.Property(e => e.ValidFrom).HasColumnName("valid_from");
                entity.Property(e => e.ValidUntil).HasColumnName("valid_until");
                entity.Property(e => e.VoucherExpiryDays).HasColumnName("voucher_expiry_days");
                entity.Property(e => e.ImageUrl).HasColumnName("image_url");
                entity.Property(e => e.IsActive).HasColumnName("is_active");
                entity.Property(e => e.CreatedAt).HasColumnName("created_at");
                entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            });
            
            // RewardRedemption entity
            modelBuilder.Entity<RewardRedemption>(entity =>
            {
                entity.ToTable("reward_redemptions");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.CustomerId).HasColumnName("customer_id");
                entity.Property(e => e.RewardId).HasColumnName("reward_id");
                entity.Property(e => e.PointsSpent).HasColumnName("points_spent");
                entity.Property(e => e.Status).HasColumnName("status");
                entity.Property(e => e.VoucherGenerated).HasColumnName("voucher_generated");
                entity.Property(e => e.VoucherExpiresAt).HasColumnName("voucher_expires_at");
                entity.Property(e => e.FulfillmentStatus).HasColumnName("fulfillment_status");
                entity.Property(e => e.FulfillmentDate).HasColumnName("fulfillment_date");
                entity.Property(e => e.RedeemedAt).HasColumnName("redeemed_at");
                entity.Property(e => e.CompletedAt).HasColumnName("completed_at");
                entity.Property(e => e.Notes).HasColumnName("notes");
                entity.HasOne(e => e.Reward).WithMany().HasForeignKey(e => e.RewardId);
            });
        }
    }
}
