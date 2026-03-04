using InventoryService.Application.DTOs;
using InventoryService.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace InventoryService.Application.Services;

public class InventoryCheckService : IInventoryCheckService
{
    private readonly IInventoryCheckRepository _inventoryCheckRepository;
    private readonly ILogger<InventoryCheckService> _logger;

    public InventoryCheckService(
        IInventoryCheckRepository inventoryCheckRepository,
        ILogger<InventoryCheckService> logger)
    {
        _inventoryCheckRepository = inventoryCheckRepository;
        _logger = logger;
    }

    public async Task<IEnumerable<InventoryCheckListDto>> GetAllInventoryChecksAsync()
    {
        try
        {
            var checks = await _inventoryCheckRepository.GetAllAsync();
            
            return checks.Select(c => new InventoryCheckListDto
            {
                Id = c.Id,
                CheckNumber = c.CheckNumber,
                LocationType = c.LocationType,
                LocationId = c.LocationId,
                CheckType = c.CheckType,
                CheckDate = c.CheckDate,
                CheckedBy = c.CheckedBy,
                Status = c.Status,
                TotalDiscrepancies = c.TotalDiscrepancies,
                CreatedAt = c.CreatedAt
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all inventory checks");
            throw;
        }
    }

    public async Task<InventoryCheckDto?> GetInventoryCheckByIdAsync(Guid id)
    {
        try
        {
            var check = await _inventoryCheckRepository.GetByIdAsync(id);
            
            if (check == null)
            {
                return null;
            }

            return new InventoryCheckDto
            {
                Id = check.Id,
                CheckNumber = check.CheckNumber,
                LocationType = check.LocationType,
                LocationId = check.LocationId,
                CheckType = check.CheckType,
                CheckDate = check.CheckDate,
                CheckedBy = check.CheckedBy,
                Status = check.Status,
                TotalDiscrepancies = check.TotalDiscrepancies,
                Notes = check.Notes,
                CreatedAt = check.CreatedAt,
                Items = check.InventoryCheckItems.Select(item => new InventoryCheckItemDto
                {
                    Id = item.Id,
                    ProductId = item.ProductId,
                    SystemQuantity = item.SystemQuantity,
                    ActualQuantity = item.ActualQuantity,
                    Difference = item.Difference,
                    Note = item.Note
                }).ToList()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting inventory check by id: {Id}", id);
            throw;
        }
    }
}
