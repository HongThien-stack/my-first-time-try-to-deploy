using Microsoft.EntityFrameworkCore;
using PromotionService.Application.DTOs;
using PromotionService.Application.Interfaces;
using PromotionService.Domain.Entities;
using System.Text.Json;

namespace PromotionService.Application.Services
{
    public class PromotionEngineService : IPromotionEngineService
    {
        private readonly IPromotionDbContext _context;

        public PromotionEngineService(IPromotionDbContext context)
        {
            _context = context;
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

        private class CategoryRuleCondition
        {
            public List<Guid>? category_ids { get; set; }
        }
    }
}
