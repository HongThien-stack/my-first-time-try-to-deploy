using PosService.Application.DTOs.External;

namespace PosService.Application.Interfaces.Http
{
    public interface IProductServiceClient
    {
        Task<List<ProductDetailsDto>> GetProductDetailsBatchAsync(IEnumerable<Guid> productIds);
        Task<(List<ProductDetailsDto> Items, int TotalCount)?> SearchProductsAsync(string keyword, int pageNumber, int pageSize);
    }
}
