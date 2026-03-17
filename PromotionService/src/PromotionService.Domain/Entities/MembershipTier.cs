using PromotionService.Domain.Common;

namespace PromotionService.Domain.Entities
{
    public class MembershipTier : BaseEntity
    {
        public string TierName { get; set; } = string.Empty; // BRONZE, SILVER, GOLD
        public int TierLevel { get; set; }
        public int MinPoints { get; set; }
        public decimal MinPurchases { get; set; }
        public decimal DiscountPercentage { get; set; } = 0;
        public decimal PointsMultiplier { get; set; } = 1.0m;
        public int BirthdayBonusPoints { get; set; } = 0;
        public string? Color { get; set; }
        public string? IconUrl { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
