using PromotionService.Domain.Common;

namespace PromotionService.Domain.Entities
{
    public class CustomerLoyalty : BaseEntity
    {
        public Guid CustomerId { get; set; }
        public Guid MembershipTierId { get; set; }
        public MembershipTier? MembershipTier { get; set; }
        public int TotalPoints { get; set; } = 0;
        public int AvailablePoints { get; set; } = 0;
        public int UsedPoints { get; set; } = 0;
        public int ExpiredPoints { get; set; } = 0;
        public decimal TotalPurchases { get; set; } = 0;
        public int PurchaseCount { get; set; } = 0;
        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastPurchaseAt { get; set; }
        public DateTime? TierUpgradedAt { get; set; }
    }
}
