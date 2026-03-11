using System.Net.Http.Json;
using System.Text.Json;
using InventoryService.Application.DTOs;
using InventoryService.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace InventoryService.Infrastructure.Services;

public class ProductServiceClient : IProductServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ProductServiceClient> _logger;

    // Matches the wrapper shape: { "success": true, "data": { ... } }
    private sealed class ProductApiResponse
    {
        public bool Success { get; set; }
        public ProductInfoDto? Data { get; set; }
    }

    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web);

    public ProductServiceClient(HttpClient httpClient, ILogger<ProductServiceClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<ProductInfoDto?> GetProductByIdAsync(Guid productId)
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<ProductApiResponse>(
                $"/api/Product/Get-Product-by-ID?id={productId}",
                _jsonOptions);

            return response?.Success == true ? response.Data : null;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "ProductService unavailable for product {ProductId} — unit info skipped", productId);
            return null;
        }
    }

    public async Task<Dictionary<Guid, ProductInfoDto>> GetProductsByIdsAsync(IEnumerable<Guid> productIds)
    {
        var distinct = productIds.Distinct().ToList();

        var tasks = distinct.Select(async id => (id, info: await GetProductByIdAsync(id)));
        var results = await Task.WhenAll(tasks);

        return results
            .Where(r => r.info != null)
            .ToDictionary(r => r.id, r => r.info!);
    }
}
