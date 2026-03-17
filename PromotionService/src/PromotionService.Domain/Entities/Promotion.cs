using PromotionService.Domain.Common;

namespace PromotionService.Domain.Entities
{
    public class Promotion : BaseEntity
    {
        public string PromotionCode { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string PromotionType { get; set; } = string.Empty; // PERCENTAGE | FIXED | BUY_X_GET_Y
        public decimal? DiscountPercentage { get; set; }
        public decimal? DiscountAmount { get; set; }
        public decimal? MinPurchaseAmount { get; set; }
        public decimal? MaxDiscountAmount { get; set; }
        public string ApplicableTo { get; set; } = "ALL"; // ALL | SPECIFIC_PRODUCTS | SPECIFIC_CATEGORIES
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int? UsageLimit { get; set; }
        public int UsageLimitPerCustomer { get; set; } = 1;
        public int UsageCount { get; set; } = 0;
        public bool IsActive { get; set; } = true;
        public bool IsDeleted { get; set; } = false;
        public Guid? CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        public ICollection<PromotionRule> Rules { get; set; } = new List<PromotionRule>();
    }
}
