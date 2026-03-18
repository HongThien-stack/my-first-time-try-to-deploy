using System.Text.Json;

namespace PromotionService.Application.DTOs
{
    public class UpdatePromotionRequestDto
    {
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
        public int UsageLimitPerCustomer { get; set; } = 1;
        public bool IsActive { get; set; } = true;
        public List<UpdatePromotionRuleRequestDto> Rules { get; set; } = new();
    }

    public class UpdatePromotionRuleRequestDto
    {
        public string RuleType { get; set; } = string.Empty;
        public JsonElement RuleCondition { get; set; }
    }
}
