using PromotionService.Domain.Common;

namespace PromotionService.Domain.Entities
{
    public class Reward : BaseEntity
    {
        public string RewardCode { get; set; } = string.Empty;
        public string RewardName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string RewardType { get; set; } = string.Empty; // DISCOUNT | PRODUCT
        public int PointsCost { get; set; }
        public decimal? DiscountAmount { get; set; }
        public Guid? ProductId { get; set; }
        public int? StockQuantity { get; set; }
        public int RedeemedCount { get; set; } = 0;
        public DateTime? ValidFrom { get; set; }
        public DateTime? ValidUntil { get; set; }
        public int VoucherExpiryDays { get; set; } = 30;
        public string? ImageUrl { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}
