using System.Text.Json;

namespace PromotionService.Application.DTOs
{
    public class PromotionResponseDto
    {
        public Guid Id { get; set; }
        public string PromotionCode { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string PromotionType { get; set; } = string.Empty;
        public decimal? DiscountPercentage { get; set; }
        public decimal? DiscountAmount { get; set; }
        public decimal? MinPurchaseAmount { get; set; }
        public decimal? MaxDiscountAmount { get; set; }
        public string ApplicableTo { get; set; } = "ALL";
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int? UsageLimit { get; set; }
        public int UsageLimitPerCustomer { get; set; }
        public int UsageCount { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<PromotionRuleResponseDto> Rules { get; set; } = new();
    }

    public class PromotionRuleResponseDto
    {
        public Guid Id { get; set; }
        public string RuleType { get; set; } = string.Empty;
        public JsonElement RuleCondition { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
