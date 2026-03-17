using System.Net.Http.Json;
using PosService.Application.DTOs.External;
using PosService.Application.Interfaces.Http;
using Microsoft.Extensions.Logging;

namespace PosService.Infrastructure.HttpClients
{
    public class InventoryServiceClient : IInventoryServiceClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<InventoryServiceClient> _logger;

        public InventoryServiceClient(HttpClient httpClient, ILogger<InventoryServiceClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<Dictionary<Guid, int>> GetStockLevelsBatchAsync(IEnumerable<Guid> productIds)
        {
            if (productIds == null || !productIds.Any())
            {
                return new Dictionary<Guid, int>();
            }

            try
            {
                var request = new { ProductIds = productIds };
                var response = await _httpClient.PostAsJsonAsync("/api/inventory/batch-stock", request);

                if (response.IsSuccessStatusCode)
                {
                    var stockLevels = await response.Content.ReadFromJsonAsync<Dictionary<Guid, int>>();
                    return stockLevels ?? new Dictionary<Guid, int>();
                }

                _logger.LogError("Failed to get stock levels from InventoryService. Status: {StatusCode}, Reason: {ReasonPhrase}", response.StatusCode, response.ReasonPhrase);
                return new Dictionary<Guid, int>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while calling InventoryService for stock levels.");
                return new Dictionary<Guid, int>();
            }
        }
    }
}
