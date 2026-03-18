using System.Text.Json;
using PromotionService.Application.DTOs;
using PromotionService.Domain.Entities;

namespace PromotionService.Application.Mappings
{
    public static class PromotionMappingExtensions
    {
        public static Promotion ToPromotionEntity(this CreatePromotionRequestDto request)
        {
            return new Promotion
            {
                Id = Guid.NewGuid(),
                PromotionCode = request.PromotionCode.Trim(),
                Name = request.Name.Trim(),
                Description = request.Description,
                PromotionType = request.PromotionType.Trim().ToUpperInvariant(),
                DiscountPercentage = request.DiscountPercentage,
                DiscountAmount = request.DiscountAmount,
                MinPurchaseAmount = request.MinPurchaseAmount,
                MaxDiscountAmount = request.MaxDiscountAmount,
                ApplicableTo = string.IsNullOrWhiteSpace(request.ApplicableTo)
                    ? "ALL"
                    : request.ApplicableTo.Trim().ToUpperInvariant(),
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                UsageLimit = request.UsageLimit,
                UsageLimitPerCustomer = request.UsageLimitPerCustomer,
                UsageCount = 0,
                IsActive = request.IsActive,
                IsDeleted = false,
                CreatedBy = request.CreatedBy,
                CreatedAt = DateTime.UtcNow
            };
        }

        public static List<PromotionRule> ToPromotionRuleEntities(this IEnumerable<CreatePromotionRuleRequestDto> rules)
        {
            return rules.Select(rule => new PromotionRule
            {
                Id = Guid.NewGuid(),
                RuleType = rule.RuleType.Trim().ToUpperInvariant(),
                RuleCondition = rule.RuleCondition.ValueKind == JsonValueKind.Undefined
                    ? "{}"
                    : rule.RuleCondition.GetRawText(),
                CreatedAt = DateTime.UtcNow
            }).ToList();
        }

        public static void ApplyUpdate(this Promotion promotion, UpdatePromotionRequestDto request)
        {
            promotion.PromotionCode = request.PromotionCode.Trim().ToUpperInvariant();
            promotion.Name = request.Name.Trim();
            promotion.Description = request.Description;
            promotion.PromotionType = request.PromotionType.Trim().ToUpperInvariant();
            promotion.DiscountPercentage = request.DiscountPercentage;
            promotion.DiscountAmount = request.DiscountAmount;
            promotion.MinPurchaseAmount = request.MinPurchaseAmount;
            promotion.MaxDiscountAmount = request.MaxDiscountAmount;
            promotion.ApplicableTo = string.IsNullOrWhiteSpace(request.ApplicableTo)
                ? "ALL"
                : request.ApplicableTo.Trim().ToUpperInvariant();
            promotion.StartDate = request.StartDate;
            promotion.EndDate = request.EndDate;
            promotion.UsageLimit = request.UsageLimit;
            promotion.UsageLimitPerCustomer = request.UsageLimitPerCustomer;
            promotion.IsActive = request.IsActive;
            promotion.UpdatedAt = DateTime.UtcNow;
        }

        public static List<PromotionRule> ToPromotionRuleEntities(this IEnumerable<UpdatePromotionRuleRequestDto> rules)
        {
            return rules.Select(rule => new PromotionRule
            {
                Id = Guid.NewGuid(),
                RuleType = rule.RuleType.Trim().ToUpperInvariant(),
                RuleCondition = rule.RuleCondition.ValueKind == JsonValueKind.Undefined
                    ? "{}"
                    : rule.RuleCondition.GetRawText(),
                CreatedAt = DateTime.UtcNow
            }).ToList();
        }

        public static PromotionResponseDto ToPromotionResponseDto(this Promotion promotion)
        {
            return new PromotionResponseDto
            {
                Id = promotion.Id,
                PromotionCode = promotion.PromotionCode,
                Name = promotion.Name,
                Description = promotion.Description,
                PromotionType = promotion.PromotionType,
                DiscountPercentage = promotion.DiscountPercentage,
                DiscountAmount = promotion.DiscountAmount,
                MinPurchaseAmount = promotion.MinPurchaseAmount,
                MaxDiscountAmount = promotion.MaxDiscountAmount,
                ApplicableTo = promotion.ApplicableTo,
                StartDate = promotion.StartDate,
                EndDate = promotion.EndDate,
                UsageLimit = promotion.UsageLimit,
                UsageLimitPerCustomer = promotion.UsageLimitPerCustomer,
                UsageCount = promotion.UsageCount,
                IsActive = promotion.IsActive,
                CreatedAt = promotion.CreatedAt,
                Rules = promotion.Rules.Select(rule => new PromotionRuleResponseDto
                {
                    Id = rule.Id,
                    RuleType = rule.RuleType,
                    RuleCondition = ParseJsonElement(rule.RuleCondition),
                    CreatedAt = rule.CreatedAt
                }).ToList()
            };
        }

        public static DeletePromotionResponseDto ToDeletePromotionResponseDto(this Promotion promotion)
        {
            return new DeletePromotionResponseDto
            {
                Id = promotion.Id,
                IsDeleted = promotion.IsDeleted,
                IsActive = promotion.IsActive,
                UpdatedAt = promotion.UpdatedAt
            };
        }

        public static PromotionListItemDto ToPromotionListItemDto(this Promotion promotion)
        {
            return new PromotionListItemDto
            {
                Id = promotion.Id,
                PromotionCode = promotion.PromotionCode,
                Name = promotion.Name,
                Description = promotion.Description,
                PromotionType = promotion.PromotionType,
                DiscountPercentage = promotion.DiscountPercentage,
                DiscountAmount = promotion.DiscountAmount,
                MinPurchaseAmount = promotion.MinPurchaseAmount,
                MaxDiscountAmount = promotion.MaxDiscountAmount,
                ApplicableTo = promotion.ApplicableTo,
                StartDate = promotion.StartDate,
                EndDate = promotion.EndDate,
                UsageLimit = promotion.UsageLimit,
                UsageLimitPerCustomer = promotion.UsageLimitPerCustomer,
                UsageCount = promotion.UsageCount,
                IsActive = promotion.IsActive,
                CreatedAt = promotion.CreatedAt,
                UpdatedAt = promotion.UpdatedAt,
                Rules = promotion.Rules.Select(rule => new PromotionRuleResponseDto
                {
                    Id = rule.Id,
                    RuleType = rule.RuleType,
                    RuleCondition = ParseJsonElement(rule.RuleCondition),
                    CreatedAt = rule.CreatedAt
                }).ToList()
            };
        }

        private static JsonElement ParseJsonElement(string rawJson)
        {
            if (string.IsNullOrWhiteSpace(rawJson))
            {
                using var emptyDoc = JsonDocument.Parse("{}");
                return emptyDoc.RootElement.Clone();
            }

            try
            {
                using var doc = JsonDocument.Parse(rawJson);
                return doc.RootElement.Clone();
            }
            catch (JsonException)
            {
                using var fallbackDoc = JsonDocument.Parse("{}");
                return fallbackDoc.RootElement.Clone();
            }
        }
    }
}
