using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using PosService.Application.DTOs.External;
using PosService.Application.Interfaces.Http;
using System.Net.Http.Json;

namespace PosService.Infrastructure.HttpClients
{
    public class ProductServiceClient : IProductServiceClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ProductServiceClient> _logger;

        public ProductServiceClient(HttpClient httpClient, ILogger<ProductServiceClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<List<ProductDetailsDto>> GetProductDetailsBatchAsync(IEnumerable<Guid> productIds)
        {
            var request = new { productIds };
            var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

            try
            {
                var response = await _httpClient.PostAsync("/api/Product/details-batch", content);
                response.EnsureSuccessStatusCode();

                var responseStream = await response.Content.ReadAsStreamAsync();
                var result = await JsonSerializer.DeserializeAsync<ApiResponse<List<ProductDetailsDto>>>(responseStream, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                return result?.Data ?? new List<ProductDetailsDto>();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request to ProductService failed.");
                throw;
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to deserialize response from ProductService.");
                throw;
            }
        }

        public async Task<(List<ProductDetailsDto> Items, int TotalCount)?> SearchProductsAsync(string keyword, int pageNumber, int pageSize)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/api/products/search?keyword={keyword}&pageNumber={pageNumber}&pageSize={pageSize}");

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<PagedSearchResult<ProductDetailsDto>>();
                    return (result?.Items ?? new List<ProductDetailsDto>(), result?.TotalCount ?? 0);
                }

                _logger.LogError("Failed to search products from ProductService. Status: {StatusCode}, Reason: {ReasonPhrase}", response.StatusCode, response.ReasonPhrase);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while calling ProductService for product search.");
                return null;
            }
        }
    }

    // A generic wrapper to match the common API response structure
    internal class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public T? Data { get; set; }
    }

    public class PagedSearchResult<T>
    {
        public List<T> Items { get; set; } = new List<T>();
        public int TotalCount { get; set; }
    }
}
