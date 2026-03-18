using PromotionService.Domain.Entities;

namespace PromotionService.Application.Interfaces
{
    public interface IPromotionRepository
    {
        Task<bool> PromotionCodeExistsAsync(string promotionCode, CancellationToken cancellationToken = default);
        Task<bool> PromotionCodeExistsExceptIdAsync(string promotionCode, Guid excludedId, CancellationToken cancellationToken = default);
        Task<List<Promotion>> GetPromotionsAsync(string? promotionType, CancellationToken cancellationToken = default);
        Task<Promotion> CreatePromotionWithRulesAsync(
            Promotion promotion,
            IEnumerable<PromotionRule> rules,
            CancellationToken cancellationToken = default);
        Task<Promotion?> GetByIdWithRulesAsync(Guid id, CancellationToken cancellationToken = default);
        Task<Promotion> UpdatePromotionWithRulesAsync(
            Promotion promotion,
            IEnumerable<PromotionRule> newRules,
            CancellationToken cancellationToken = default);
        Task<Promotion> SoftDeleteAsync(Promotion promotion, CancellationToken cancellationToken = default);
    }
}
