using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using PosService.Application.DTOs.External;
using PosService.Application.Interfaces.Http;

namespace PosService.Infrastructure.HttpClients
{
    public class PromotionServiceClient : IPromotionServiceClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<PromotionServiceClient> _logger;

        public PromotionServiceClient(HttpClient httpClient, ILogger<PromotionServiceClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<PromotionCalculationResultDto> CalculatePromotionsAsync(PromotionCalculationRequestDto request)
        {
            // Convert PromotionCalculationRequestDto to the JSON format expected by PromotionService
            var cartPayload = new
            {
                customerId = request.CustomerId,
                voucherCode = request.VoucherCode,
                items = request.Items
            };
            
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            
            var content = new StringContent(JsonSerializer.Serialize(cartPayload, options), Encoding.UTF8, "application/json");

            try
            {
                var response = await _httpClient.PostAsync("/api/promotions/calculate", content);
                response.EnsureSuccessStatusCode();

                var responseStream = await response.Content.ReadAsStreamAsync();
                var result = await JsonSerializer.DeserializeAsync<PromotionCalculationResultDto>(responseStream, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                return result ?? BuildFallbackResult(request);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogWarning(ex, "PromotionService is unavailable. Continuing without promotions.");
                return BuildFallbackResult(request);
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(ex, "PromotionService returned an invalid payload. Continuing without promotions.");
                return BuildFallbackResult(request);
            }
        }

        private static PromotionCalculationResultDto BuildFallbackResult(PromotionCalculationRequestDto request)
        {
            var subtotal = request.Items.Sum(item => item.UnitPrice * item.Quantity);

            return new PromotionCalculationResultDto
            {
                Subtotal = subtotal,
                TotalDiscountAmount = 0,
                TotalAmount = subtotal,
                PointsEarned = (int)Math.Floor(subtotal / 1000),
                AppliedPromotions = new List<AppliedPromotionDto>()
            };
        }
    }
}
