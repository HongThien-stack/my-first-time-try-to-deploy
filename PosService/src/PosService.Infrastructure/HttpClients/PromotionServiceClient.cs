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
            
            var content = new StringContent(JsonSerializer.Serialize(cartPayload), Encoding.UTF8, "application/json");

            try
            {
                var response = await _httpClient.PostAsync("/api/promotions/calculate", content);
                response.EnsureSuccessStatusCode();

                var responseStream = await response.Content.ReadAsStreamAsync();
                var result = await JsonSerializer.DeserializeAsync<PromotionCalculationResultDto>(responseStream, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                return result ?? new PromotionCalculationResultDto();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request to PromotionService failed.");
                throw;
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to deserialize response from PromotionService.");
                throw;
            }
        }
    }
}
