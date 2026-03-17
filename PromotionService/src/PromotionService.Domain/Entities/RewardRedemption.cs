using PromotionService.Domain.Common;

namespace PromotionService.Domain.Entities
{
    public class RewardRedemption : BaseEntity
    {
        public Guid CustomerId { get; set; }
        public Guid RewardId { get; set; }
        public Reward? Reward { get; set; }
        public int PointsSpent { get; set; }
        public string Status { get; set; } = "PENDING"; // PENDING | COMPLETED | CANCELLED
        public string? VoucherGenerated { get; set; }
        public DateTime? VoucherExpiresAt { get; set; }
        public string? FulfillmentStatus { get; set; } // PENDING | SHIPPED | DELIVERED
        public DateTime? FulfillmentDate { get; set; }
        public DateTime RedeemedAt { get; set; } = DateTime.UtcNow;
        public DateTime? CompletedAt { get; set; }
        public string? Notes { get; set; }
    }
}
