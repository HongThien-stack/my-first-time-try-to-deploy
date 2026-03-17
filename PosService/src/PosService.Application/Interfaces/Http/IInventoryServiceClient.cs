using PosService.Application.DTOs.External;

namespace PosService.Application.Interfaces.Http
{
    public interface IInventoryServiceClient
    {
        Task<Dictionary<Guid, int>> GetStockLevelsBatchAsync(IEnumerable<Guid> productIds);
    }
}
