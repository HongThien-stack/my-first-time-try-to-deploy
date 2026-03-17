using PromotionService.Application.DTOs;

namespace PromotionService.Application.Interfaces
{
    public interface IPromotionEngineService
    {
        Task<CalculationResultDto> CalculateDiscountsAsync(CartDto cart);
    }
}
