using System.Net.Http.Headers;
using System.Text.Json;
using PosService.Application.DTOs;

namespace PosService.API.Services;

public class InventoryServiceClient
{
    private readonly HttpClient _httpClient;

    public InventoryServiceClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IReadOnlyList<InventorySaleProductDto>> GetSaleProductsAsync(string? bearerToken, Guid storeId)
    {
        // Append storeId as a query parameter
        using var request = new HttpRequestMessage(HttpMethod.Get, $"/api/inventory?storeId={storeId}");
        if (!string.IsNullOrWhiteSpace(bearerToken))
        {
            request.Headers.Authorization = AuthenticationHeaderValue.Parse(bearerToken);
        }

        using var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadAsStringAsync();
        var wrapper = JsonSerializer.Deserialize<InventoryApiResponse>(payload, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        var data = wrapper?.Data ?? new List<InventoryItem>();

        // Inventory payload does not include product_name/is_sale, so we return saleable/available product ids.
        var result = data
            .Where(i => string.Equals(i.LocationType, "STORE", StringComparison.OrdinalIgnoreCase) && i.AvailableQuantity > 0)
            .GroupBy(i => i.ProductId)
            .Select(g => new InventorySaleProductDto
            {
                ProductId = g.Key,
                ProductName = string.Empty,
                ProductSku = null,
                AvailableQuantity = g.Sum(x => x.AvailableQuantity ?? 0)
            })
            .ToList();

        return result;
    }

    private sealed class InventoryApiResponse
    {
        public List<InventoryItem>? Data { get; set; }
    }

    private sealed class InventoryItem
    {
        public Guid ProductId { get; set; }
        public string LocationType { get; set; } = string.Empty;
        public int? AvailableQuantity { get; set; }
    }
}
