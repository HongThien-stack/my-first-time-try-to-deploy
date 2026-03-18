using System.Text.Json;

namespace PromotionService.Application.DTOs
{
    public class CreatePromotionRequestDto
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
        public Guid? CreatedBy { get; set; }
        public List<CreatePromotionRuleRequestDto> Rules { get; set; } = new();
    }

    public class CreatePromotionRuleRequestDto
    {
        public string RuleType { get; set; } = string.Empty;
        public JsonElement RuleCondition { get; set; }
    }
}
