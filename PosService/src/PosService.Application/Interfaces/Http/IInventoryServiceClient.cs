using PosService.Application.DTOs.External;

namespace PosService.Application.Interfaces.Http
{
    public interface IInventoryServiceClient
    {
        Task<Dictionary<Guid, int>> GetStockLevelsBatchAsync(IEnumerable<Guid> productIds);
        Task<bool> ReduceInventoryAsync(Guid storeId, List<(Guid ProductId, int Quantity)> items);
        Task<CheckInventoryResponseDto> CheckAvailabilityAsync(Guid storeId, List<(Guid ProductId, int Quantity)> items);
    }
}
