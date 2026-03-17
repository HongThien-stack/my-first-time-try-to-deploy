using PromotionService.Domain.Common;

namespace PromotionService.Domain.Entities
{
    public class PromotionRule : BaseEntity
    {
        public Guid PromotionId { get; set; }
        public Promotion? Promotion { get; set; }
        public string RuleType { get; set; } = string.Empty; // PRODUCT | CATEGORY | CUSTOMER_TIER
        public string RuleCondition { get; set; } = string.Empty; // JSON: {"product_ids": [...]} or {"category_ids": [...]}
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
