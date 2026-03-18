using PromotionService.Application.DTOs;

namespace PromotionService.Application.Interfaces
{
    public interface IPromotionEngineService
    {
        Task<IReadOnlyList<PromotionListItemDto>> GetPromotionsAsync(GetPromotionsQueryDto query, CancellationToken cancellationToken = default);
        Task<CalculationResultDto> CalculateDiscountsAsync(CartDto cart);
        Task<PromotionResponseDto> CreatePromotionAsync(CreatePromotionRequestDto request, CancellationToken cancellationToken = default);
        Task<PromotionResponseDto> UpdatePromotionAsync(Guid id, UpdatePromotionRequestDto request, CancellationToken cancellationToken = default);
        Task<DeletePromotionResponseDto> DeletePromotionAsync(Guid id, CancellationToken cancellationToken = default);
    }
}
