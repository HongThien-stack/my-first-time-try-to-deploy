using PosService.Application.DTOs.External;

namespace PosService.Application.Interfaces.Http
{
    public interface IPromotionServiceClient
    {
        Task<PromotionCalculationResultDto> CalculatePromotionsAsync(PromotionCalculationRequestDto request);
    }
}
