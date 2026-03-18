using Microsoft.EntityFrameworkCore;
using PromotionService.Application.DTOs;
using PromotionService.Application.Interfaces;
using PromotionService.Application.Mappings;
using PromotionService.Domain.Entities;
using System.Text.Json;

namespace PromotionService.Application.Services
{
    public class PromotionEngineService : IPromotionEngineService
    {
        private readonly IPromotionDbContext _context;
        private readonly IPromotionRepository _promotionRepository;

        public PromotionEngineService(IPromotionDbContext context, IPromotionRepository promotionRepository)
        {
            _context = context;
            _promotionRepository = promotionRepository;
        }

        public async Task<IReadOnlyList<PromotionListItemDto>> GetPromotionsAsync(GetPromotionsQueryDto query, CancellationToken cancellationToken = default)
        {
            query ??= new GetPromotionsQueryDto();

            var promotions = await _promotionRepository.GetPromotionsAsync(query.PromotionType, cancellationToken);

            if (query.IsActive.HasValue)
            {
                var now = DateTime.UtcNow;
                promotions = query.IsActive.Value
                    ? promotions.Where(p => p.IsActive && p.StartDate <= now && p.EndDate >= now).ToList()
                    : promotions.Where(p => !p.IsActive || now < p.StartDate || now > p.EndDate).ToList();
            }

            return promotions.Select(p => p.ToPromotionListItemDto()).ToList();
        }

        public async Task<PromotionResponseDto> CreatePromotionAsync(CreatePromotionRequestDto request, CancellationToken cancellationToken = default)
        {
            ValidateCreateRequest(request);

            var normalizedPromotionCode = request.PromotionCode.Trim().ToUpperInvariant();
            var isCodeExists = await _promotionRepository.PromotionCodeExistsAsync(normalizedPromotionCode, cancellationToken);
            if (isCodeExists)
            {
                throw new ArgumentException($"Promotion code '{request.PromotionCode}' already exists.");
            }

            var promotion = request.ToPromotionEntity();
            promotion.PromotionCode = normalizedPromotionCode;

            var rules = (request.Rules ?? new List<CreatePromotionRuleRequestDto>()).ToPromotionRuleEntities();
            var createdPromotion = await _promotionRepository.CreatePromotionWithRulesAsync(
                promotion,
                rules,
                cancellationToken);

            return createdPromotion.ToPromotionResponseDto();
        }

        public async Task<PromotionResponseDto> UpdatePromotionAsync(Guid id, UpdatePromotionRequestDto request, CancellationToken cancellationToken = default)
        {
            ValidateUpdateRequest(request);

            var existingPromotion = await _promotionRepository.GetByIdWithRulesAsync(id, cancellationToken);
            if (existingPromotion == null || existingPromotion.IsDeleted)
            {
                throw new KeyNotFoundException($"Promotion '{id}' was not found.");
            }

            var normalizedPromotionCode = request.PromotionCode.Trim().ToUpperInvariant();
            var isDuplicateCode = await _promotionRepository.PromotionCodeExistsExceptIdAsync(normalizedPromotionCode, id, cancellationToken);
            if (isDuplicateCode)
            {
                throw new ArgumentException($"Promotion code '{request.PromotionCode}' already exists.");
            }

            existingPromotion.ApplyUpdate(request);
            var rules = (request.Rules ?? new List<UpdatePromotionRuleRequestDto>()).ToPromotionRuleEntities();

            var updatedPromotion = await _promotionRepository.UpdatePromotionWithRulesAsync(
                existingPromotion,
                rules,
                cancellationToken);

            return updatedPromotion.ToPromotionResponseDto();
        }

        public async Task<DeletePromotionResponseDto> DeletePromotionAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var existingPromotion = await _promotionRepository.GetByIdWithRulesAsync(id, cancellationToken);
            if (existingPromotion == null)
            {
                throw new KeyNotFoundException($"Promotion '{id}' was not found.");
            }

            if (existingPromotion.IsDeleted)
            {
                throw new ArgumentException($"Promotion '{id}' is already deleted.");
            }

            existingPromotion.IsDeleted = true;
            existingPromotion.IsActive = false;
            existingPromotion.UpdatedAt = DateTime.UtcNow;

            var deletedPromotion = await _promotionRepository.SoftDeleteAsync(existingPromotion, cancellationToken);
            return deletedPromotion.ToDeletePromotionResponseDto();
        }

        public async Task<CalculationResultDto> CalculateDiscountsAsync(CartDto cart)
        {
            var result = new CalculationResultDto();
            var subtotal = cart.Items.Sum(i => i.UnitPrice * i.Quantity);
            result.Subtotal = subtotal;

            var applicablePromotions = await FindApplicablePromotions(cart);

            decimal totalDiscount = 0;

            foreach (var promo in applicablePromotions)
            {
                // Simple logic for now: Apply first found promotion
                // In a real scenario, you'd handle combined promotions, etc.
                if (promo.PromotionType == "PERCENTAGE" && promo.DiscountPercentage.HasValue)
                {
                    decimal discount = subtotal * (promo.DiscountPercentage.Value / 100);
                    if (promo.MaxDiscountAmount.HasValue && discount > promo.MaxDiscountAmount.Value)
                    {
                        discount = promo.MaxDiscountAmount.Value;
                    }
                    totalDiscount += discount;
                    result.AppliedPromotions.Add(new AppliedPromotionDto
                    {
                        PromotionId = promo.Id,
                        PromotionName = promo.Name,
                        DiscountAmount = discount
                    });
                }
                else if (promo.PromotionType == "FIXED" && promo.DiscountAmount.HasValue)
                {
                    totalDiscount += promo.DiscountAmount.Value;
                     result.AppliedPromotions.Add(new AppliedPromotionDto
                    {
                        PromotionId = promo.Id,
                        PromotionName = promo.Name,
                        DiscountAmount = promo.DiscountAmount.Value
                    });
                }
            }
            
            // This is a simplified discount distribution. A real system might distribute it proportionally.
            result.TotalDiscountAmount = totalDiscount;
            result.TotalAmount = subtotal - totalDiscount;

            // Simplified points calculation
            result.PointsEarned = (int)Math.Floor(result.TotalAmount / 1000);

            return result;
        }

        private async Task<List<Promotion>> FindApplicablePromotions(CartDto cart)
        {
            var now = DateTime.UtcNow;
            var allActivePromotions = await _context.Promotions
                .Include(p => p.Rules)
                .Where(p => p.IsActive && !p.IsDeleted && p.StartDate <= now && p.EndDate >= now)
                .ToListAsync();

            var applicablePromotions = new List<Promotion>();
            var cartCategoryIds = cart.Items.Select(i => i.CategoryId).Distinct().ToList();

            foreach (var promo in allActivePromotions)
            {
                if (promo.MinPurchaseAmount.HasValue && cart.Items.Sum(i => i.UnitPrice * i.Quantity) < promo.MinPurchaseAmount.Value)
                {
                    continue; // Skip if cart total is less than minimum required
                }

                if (promo.ApplicableTo == "ALL")
                {
                    applicablePromotions.Add(promo);
                }
                else if (promo.ApplicableTo == "SPECIFIC_CATEGORIES")
                {
                    var categoryRules = promo.Rules.Where(r => r.RuleType == "CATEGORY");
                    foreach (var rule in categoryRules)
                    {
                        var condition = JsonSerializer.Deserialize<CategoryRuleCondition>(rule.RuleCondition);
                        if (condition?.category_ids != null && condition.category_ids.Any(id => cartCategoryIds.Contains(id)))
                        {
                            applicablePromotions.Add(promo);
                            break; 
                        }
                    }
                }
                // Add logic for other types like SPECIFIC_PRODUCTS if needed
            }

            return applicablePromotions;
        }

        private static void ValidateCreateRequest(CreatePromotionRequestDto request)
        {
            if (request == null)
            {
                throw new ArgumentException("Request body is required.");
            }

            if (string.IsNullOrWhiteSpace(request.PromotionCode))
            {
                throw new ArgumentException("PromotionCode is required.");
            }

            if (string.IsNullOrWhiteSpace(request.Name))
            {
                throw new ArgumentException("Name is required.");
            }

            if (string.IsNullOrWhiteSpace(request.PromotionType))
            {
                throw new ArgumentException("PromotionType is required.");
            }

            if (request.StartDate >= request.EndDate)
            {
                throw new ArgumentException("StartDate must be earlier than EndDate.");
            }

            if (request.UsageLimit.HasValue && request.UsageLimit.Value <= 0)
            {
                throw new ArgumentException("UsageLimit must be greater than 0 when provided.");
            }

            if (request.UsageLimitPerCustomer <= 0)
            {
                throw new ArgumentException("UsageLimitPerCustomer must be greater than 0.");
            }

            if (request.MinPurchaseAmount.HasValue && request.MinPurchaseAmount.Value < 0)
            {
                throw new ArgumentException("MinPurchaseAmount cannot be negative.");
            }

            if (request.MaxDiscountAmount.HasValue && request.MaxDiscountAmount.Value < 0)
            {
                throw new ArgumentException("MaxDiscountAmount cannot be negative.");
            }

            if (request.Rules != null && request.Rules.Any(r => string.IsNullOrWhiteSpace(r.RuleType)))
            {
                throw new ArgumentException("Each rule must have RuleType.");
            }

            ValidatePromotionTypeFields(request);
        }

        private static void ValidatePromotionTypeFields(CreatePromotionRequestDto request)
        {
            var promotionType = request.PromotionType.Trim().ToUpperInvariant();

            switch (promotionType)
            {
                case "PERCENTAGE":
                    if (!request.DiscountPercentage.HasValue || request.DiscountPercentage <= 0 || request.DiscountPercentage > 100)
                    {
                        throw new ArgumentException("DiscountPercentage must be in range (0, 100] for PERCENTAGE promotions.");
                    }

                    if (request.DiscountAmount.HasValue)
                    {
                        throw new ArgumentException("DiscountAmount must be null for PERCENTAGE promotions.");
                    }
                    break;

                case "FIXED":
                    if (!request.DiscountAmount.HasValue || request.DiscountAmount <= 0)
                    {
                        throw new ArgumentException("DiscountAmount must be greater than 0 for FIXED promotions.");
                    }

                    if (request.DiscountPercentage.HasValue)
                    {
                        throw new ArgumentException("DiscountPercentage must be null for FIXED promotions.");
                    }
                    break;

                case "BUY_X_GET_Y":
                case "FREE_SHIPPING":
                    if (request.DiscountPercentage.HasValue || request.DiscountAmount.HasValue)
                    {
                        throw new ArgumentException("DiscountPercentage and DiscountAmount must be null for BUY_X_GET_Y or FREE_SHIPPING promotions.");
                    }
                    break;

                default:
                    throw new ArgumentException("PromotionType is invalid. Supported values: PERCENTAGE, FIXED, BUY_X_GET_Y, FREE_SHIPPING.");
            }
        }

        private static void ValidateUpdateRequest(UpdatePromotionRequestDto request)
        {
            if (request == null)
            {
                throw new ArgumentException("Request body is required.");
            }

            if (string.IsNullOrWhiteSpace(request.PromotionCode))
            {
                throw new ArgumentException("PromotionCode is required.");
            }

            if (string.IsNullOrWhiteSpace(request.Name))
            {
                throw new ArgumentException("Name is required.");
            }

            if (string.IsNullOrWhiteSpace(request.PromotionType))
            {
                throw new ArgumentException("PromotionType is required.");
            }

            if (request.StartDate >= request.EndDate)
            {
                throw new ArgumentException("StartDate must be earlier than EndDate.");
            }

            if (request.UsageLimit.HasValue && request.UsageLimit.Value <= 0)
            {
                throw new ArgumentException("UsageLimit must be greater than 0 when provided.");
            }

            if (request.UsageLimitPerCustomer <= 0)
            {
                throw new ArgumentException("UsageLimitPerCustomer must be greater than 0.");
            }

            if (request.MinPurchaseAmount.HasValue && request.MinPurchaseAmount.Value < 0)
            {
                throw new ArgumentException("MinPurchaseAmount cannot be negative.");
            }

            if (request.MaxDiscountAmount.HasValue && request.MaxDiscountAmount.Value < 0)
            {
                throw new ArgumentException("MaxDiscountAmount cannot be negative.");
            }

            if (request.Rules != null && request.Rules.Any(r => string.IsNullOrWhiteSpace(r.RuleType)))
            {
                throw new ArgumentException("Each rule must have RuleType.");
            }

            ValidatePromotionTypeFields(request.PromotionType, request.DiscountPercentage, request.DiscountAmount);
        }

        private static void ValidatePromotionTypeFields(string promotionTypeInput, decimal? discountPercentage, decimal? discountAmount)
        {
            var promotionType = promotionTypeInput.Trim().ToUpperInvariant();

            switch (promotionType)
            {
                case "PERCENTAGE":
                    if (!discountPercentage.HasValue || discountPercentage <= 0 || discountPercentage > 100)
                    {
                        throw new ArgumentException("DiscountPercentage must be in range (0, 100] for PERCENTAGE promotions.");
                    }

                    if (discountAmount.HasValue)
                    {
                        throw new ArgumentException("DiscountAmount must be null for PERCENTAGE promotions.");
                    }
                    break;

                case "FIXED":
                    if (!discountAmount.HasValue || discountAmount <= 0)
                    {
                        throw new ArgumentException("DiscountAmount must be greater than 0 for FIXED promotions.");
                    }

                    if (discountPercentage.HasValue)
                    {
                        throw new ArgumentException("DiscountPercentage must be null for FIXED promotions.");
                    }
                    break;

                case "BUY_X_GET_Y":
                case "FREE_SHIPPING":
                    if (discountPercentage.HasValue || discountAmount.HasValue)
                    {
                        throw new ArgumentException("DiscountPercentage and DiscountAmount must be null for BUY_X_GET_Y or FREE_SHIPPING promotions.");
                    }
                    break;

                default:
                    throw new ArgumentException("PromotionType is invalid. Supported values: PERCENTAGE, FIXED, BUY_X_GET_Y, FREE_SHIPPING.");
            }
        }

        private class CategoryRuleCondition
        {
            public List<Guid>? category_ids { get; set; }
        }
    }
}
