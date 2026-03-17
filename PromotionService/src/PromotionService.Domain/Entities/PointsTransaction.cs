using PromotionService.Domain.Common;

namespace PromotionService.Domain.Entities
{
    public class PointsTransaction : BaseEntity
    {
        public Guid CustomerId { get; set; }
        public string TransactionType { get; set; } = string.Empty; // EARNED | REDEEMED | EXPIRED | ADJUSTED | BONUS
        public int Points { get; set; }
        public Guid? SaleId { get; set; }
        public Guid? RedemptionId { get; set; }
        public int BalanceBefore { get; set; }
        public int BalanceAfter { get; set; }
        public string? Description { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public Guid? CreatedBy { get; set; }
    }
}
