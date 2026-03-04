using InventoryService.Application.DTOs;
using InventoryService.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace InventoryService.Application.Services;

public class BatchQueryService : IBatchQueryService
{
    private readonly IProductBatchQueryRepository _productBatchQueryRepository;
    private readonly ILogger<BatchQueryService> _logger;

    public BatchQueryService(
        IProductBatchQueryRepository productBatchQueryRepository,
        ILogger<BatchQueryService> logger)
    {
        _productBatchQueryRepository = productBatchQueryRepository;
        _logger = logger;
    }

    public async Task<BatchDetailDto?> GetBatchByIdAsync(Guid id)
    {
        _logger.LogInformation("Getting batch detail by ID: {BatchId}", id);
        return await _productBatchQueryRepository.GetBatchDetailByIdAsync(id);
    }

    public async Task<IEnumerable<ExpiringSoonBatchDto>> GetExpiringSoonBatchesAsync()
    {
        _logger.LogInformation("Getting expiring soon batches");
        return await _productBatchQueryRepository.GetExpiringSoonBatchesAsync();
    }
}
