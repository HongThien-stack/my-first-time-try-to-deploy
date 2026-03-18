using Microsoft.EntityFrameworkCore;
using PromotionService.Application.Interfaces;
using PromotionService.Domain.Entities;
using PromotionService.Infrastructure.Persistence;

namespace PromotionService.Infrastructure.Repositories
{
    public class PromotionRepository : IPromotionRepository
    {
        private readonly PromotionDbContext _context;

        public PromotionRepository(PromotionDbContext context)
        {
            _context = context;
        }

        public async Task<bool> PromotionCodeExistsAsync(string promotionCode, CancellationToken cancellationToken = default)
        {
            var normalizedCode = promotionCode.Trim().ToUpperInvariant();
            return await _context.Promotions
                .AnyAsync(p => !p.IsDeleted && p.PromotionCode.ToUpper() == normalizedCode, cancellationToken);
        }

        public async Task<bool> PromotionCodeExistsExceptIdAsync(string promotionCode, Guid excludedId, CancellationToken cancellationToken = default)
        {
            var normalizedCode = promotionCode.Trim().ToUpperInvariant();
            return await _context.Promotions
                .AnyAsync(
                    p => !p.IsDeleted && p.Id != excludedId && p.PromotionCode.ToUpper() == normalizedCode,
                    cancellationToken);
        }

        public async Task<List<Promotion>> GetPromotionsAsync(string? promotionType, CancellationToken cancellationToken = default)
        {
            var query = _context.Promotions
                .AsNoTracking()
                .Include(p => p.Rules)
                .Where(p => !p.IsDeleted);

            if (!string.IsNullOrWhiteSpace(promotionType))
            {
                var normalizedType = promotionType.Trim().ToUpperInvariant();
                query = query.Where(p => p.PromotionType.ToUpper() == normalizedType);
            }

            return await query
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<Promotion> CreatePromotionWithRulesAsync(
            Promotion promotion,
            IEnumerable<PromotionRule> rules,
            CancellationToken cancellationToken = default)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                await _context.Promotions.AddAsync(promotion, cancellationToken);

                var promotionRules = rules.ToList();
                foreach (var rule in promotionRules)
                {
                    rule.PromotionId = promotion.Id;
                }

                if (promotionRules.Count > 0)
                {
                    await _context.PromotionRules.AddRangeAsync(promotionRules, cancellationToken);
                }

                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                promotion.Rules = promotionRules;
                return promotion;
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }

        public async Task<Promotion?> GetByIdWithRulesAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Promotions
                .Include(p => p.Rules)
                .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        }

        public async Task<Promotion> UpdatePromotionWithRulesAsync(
            Promotion promotion,
            IEnumerable<PromotionRule> newRules,
            CancellationToken cancellationToken = default)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                var existingRules = await _context.PromotionRules
                    .Where(r => r.PromotionId == promotion.Id)
                    .ToListAsync(cancellationToken);

                if (existingRules.Count > 0)
                {
                    _context.PromotionRules.RemoveRange(existingRules);
                }

                var rulesToInsert = newRules.ToList();
                foreach (var rule in rulesToInsert)
                {
                    rule.PromotionId = promotion.Id;
                }

                if (rulesToInsert.Count > 0)
                {
                    await _context.PromotionRules.AddRangeAsync(rulesToInsert, cancellationToken);
                }

                _context.Promotions.Update(promotion);

                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                promotion.Rules = rulesToInsert;
                return promotion;
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }

        public async Task<Promotion> SoftDeleteAsync(Promotion promotion, CancellationToken cancellationToken = default)
        {
            _context.Promotions.Update(promotion);
            await _context.SaveChangesAsync(cancellationToken);
            return promotion;
        }
    }
}
