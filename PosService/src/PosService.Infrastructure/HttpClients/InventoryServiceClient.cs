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

        /// <summary>
        /// Trừ tồn kho sau khi sale completed (CASH) hoặc MOMO success
        /// </summary>
        public async Task<bool> ReduceInventoryAsync(Guid storeId, List<(Guid ProductId, int Quantity)> items)
        {
            if (storeId == Guid.Empty || items == null || !items.Any())
            {
                _logger.LogWarning("Invalid reduce inventory request: StoreId={StoreId}, ItemCount={ItemCount}", storeId, items?.Count ?? 0);
                return false;
            }

            try
            {
                var request = new
                {
                    storeId = storeId,
                    items = items.Select(i => new { productId = i.ProductId, quantity = i.Quantity }).ToList()
                };

                _logger.LogInformation("Calling InventoryService to reduce inventory for store {StoreId} with {ItemCount} items", storeId, items.Count);

                var response = await _httpClient.PostAsJsonAsync("/api/inventory/reduce", request);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Successfully reduced inventory for store {StoreId}: {ItemCount} items", storeId, items.Count);
                    return true;
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Failed to reduce inventory. Status: {StatusCode}, Response: {Response}", response.StatusCode, errorContent);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while calling InventoryService to reduce inventory for store {StoreId}", storeId);
                return false;
            }
        }

        /// <summary>
        /// Kiểm tra xem store có đủ tồn kho cho các sản phẩm yêu cầu
        /// Trả về danh sách các sản phẩm không đủ tồn kho kèm tên sản phẩm
        /// </summary>
        public async Task<CheckInventoryResponseDto> CheckAvailabilityAsync(Guid storeId, List<(Guid ProductId, int Quantity)> items)
        {
            var response = new CheckInventoryResponseDto { IsAvailable = true, UnavailableItems = new List<UnavailableItemDto>() };

            if (storeId == Guid.Empty || items == null || !items.Any())
            {
                _logger.LogWarning("Invalid check inventory request: StoreId={StoreId}, ItemCount={ItemCount}", storeId, items?.Count ?? 0);
                return response;
            }

            try
            {
                var request = new
                {
                    storeId = storeId,
                    items = items.Select(i => new { productId = i.ProductId, quantity = i.Quantity }).ToList()
                };

                _logger.LogInformation("Calling InventoryService to check availability for store {StoreId} with {ItemCount} items", storeId, items.Count);

                var httpResponse = await _httpClient.PostAsJsonAsync("/api/inventory/check-availability", request);

                if (httpResponse.IsSuccessStatusCode)
                {
                    var result = await httpResponse.Content.ReadFromJsonAsync<CheckInventoryResponseDto>();
                    _logger.LogInformation("Checked inventory availability for store {StoreId}: IsAvailable={IsAvailable}, UnavailableItems={Count}", 
                        storeId, result?.IsAvailable ?? false, result?.UnavailableItems.Count ?? 0);
                    return result ?? response;
                }

                var errorContent = await httpResponse.Content.ReadAsStringAsync();
                _logger.LogError("Failed to check inventory availability. Status: {StatusCode}, Response: {Response}", httpResponse.StatusCode, errorContent);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while calling InventoryService to check availability for store {StoreId}", storeId);
                return response;
            }
        }
    }
}
