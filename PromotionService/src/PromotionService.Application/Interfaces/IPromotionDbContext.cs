using Microsoft.EntityFrameworkCore;
using PromotionService.Domain.Entities;

namespace PromotionService.Application.Interfaces
{
    public interface IPromotionDbContext
    {
        DbSet<Promotion> Promotions { get; }
        DbSet<PromotionRule> PromotionRules { get; }
        DbSet<Voucher> Vouchers { get; }
        DbSet<MembershipTier> MembershipTiers { get; }
        DbSet<CustomerLoyalty> CustomerLoyalties { get; }
        DbSet<PointsTransaction> PointsTransactions { get; }
        DbSet<Reward> Rewards { get; }
        DbSet<RewardRedemption> RewardRedemptions { get; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    }
}
