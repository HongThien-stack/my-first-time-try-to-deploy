using InventoryService.Application.DTOs;
using InventoryService.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace InventoryService.API.Controllers;

[ApiController]
[Route("api/inventory-history")]
public class InventoryHistoryController : ControllerBase
{
    private readonly IInventoryHistoryRepository _historyRepository;
    private readonly IInventoryLogRepository _logRepository;
    private readonly ILogger<InventoryHistoryController> _logger;

    public InventoryHistoryController(
        IInventoryHistoryRepository historyRepository,
        IInventoryLogRepository logRepository,
        ILogger<InventoryHistoryController> logger)
    {
        _historyRepository = historyRepository;
        _logRepository = logRepository;
        _logger = logger;
    }

    /// <summary>
    /// Get inventory history snapshots
    /// </summary>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Items per page (default: 10, max: 100)</param>
    /// <param name="productId">Filter by product ID</param>
    /// <param name="locationId">Filter by location ID</param>
    /// <param name="dateFrom">Filter from date (YYYY-MM-DD)</param>
    /// <param name="dateTo">Filter to date (YYYY-MM-DD)</param>
    /// <returns>Paginated list of inventory history records</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetHistory(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] Guid? productId = null,
        [FromQuery] Guid? locationId = null,
        [FromQuery] DateTime? dateFrom = null,
        [FromQuery] DateTime? dateTo = null)
    {
        try
        {
            if (page < 1 || pageSize < 1 || pageSize > 100)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Page must be >= 1 and pageSize must be between 1 and 100"
                });
            }

            var (history, totalCount) = await _historyRepository.GetHistoryAsync(
                page, pageSize, productId, locationId, dateFrom, dateTo);

            var historyDtos = history.Select(h => new InventoryHistoryDto
            {
                Id = h.Id,
                InventoryId = h.InventoryId,
                ProductId = h.ProductId,
                LocationType = h.LocationType,
                LocationId = h.LocationId,
                SnapshotDate = h.SnapshotDate,
                Quantity = h.Quantity,
                ReservedQuantity = h.ReservedQuantity,
                AvailableQuantity = h.AvailableQuantity,
                CreatedAt = h.CreatedAt
            }).ToList();

            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            return Ok(new PaginatedResponseDto<InventoryHistoryDto>
            {
                Data = historyDtos,
                Pagination = new PaginationMetadata
                {
                    CurrentPage = page,
                    PageSize = pageSize,
                    TotalCount = totalCount,
                    TotalPages = totalPages
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting inventory history");
            return StatusCode(500, new
            {
                success = false,
                message = "An error occurred while retrieving inventory history",
                error = ex.Message
            });
        }
    }

    /// <summary>
    /// Get latest inventory snapshots
    /// </summary>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Items per page (default: 10, max: 100)</param>
    /// <returns>Paginated latest snapshot for each inventory record</returns>
    [HttpGet("latest")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLatestSnapshots(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        try
        {
            if (page < 1 || pageSize < 1 || pageSize > 100)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Page must be >= 1 and pageSize must be between 1 and 100"
                });
            }

            var (history, totalCount) = await _historyRepository.GetLatestSnapshotsAsync(page, pageSize);

            var historyDtos = history.Select(h => new InventoryHistoryDto
            {
                Id = h.Id,
                InventoryId = h.InventoryId,
                ProductId = h.ProductId,
                LocationType = h.LocationType,
                LocationId = h.LocationId,
                SnapshotDate = h.SnapshotDate,
                Quantity = h.Quantity,
                ReservedQuantity = h.ReservedQuantity,
                AvailableQuantity = h.AvailableQuantity,
                CreatedAt = h.CreatedAt
            }).ToList();

            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            return Ok(new PaginatedResponseDto<InventoryHistoryDto>
            {
                Data = historyDtos,
                Pagination = new PaginationMetadata
                {
                    CurrentPage = page,
                    PageSize = pageSize,
                    TotalCount = totalCount,
                    TotalPages = totalPages
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting latest inventory snapshots");
            return StatusCode(500, new
            {
                success = false,
                message = "An error occurred while retrieving latest snapshots",
                error = ex.Message
            });
        }
    }

    /// <summary>
    /// Get inventory audit logs
    /// </summary>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Items per page (default: 10, max: 100)</param>
    /// <param name="inventoryId">Filter by inventory ID</param>
    /// <param name="productId">Filter by product ID</param>
    /// <param name="action">Filter by action (comma-separated: ADJUST,RECEIVE,TRANSFER,SALE,DAMAGE)</param>
    /// <param name="performedBy">Filter by user who performed the action</param>
    /// <param name="dateFrom">Filter from date</param>
    /// <param name="dateTo">Filter to date</param>
    /// <returns>Paginated list of inventory audit logs</returns>
    [HttpGet("~/api/inventory-logs")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLogs(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] Guid? inventoryId = null,
        [FromQuery] Guid? productId = null,
        [FromQuery] string? action = null,
        [FromQuery] Guid? performedBy = null,
        [FromQuery] DateTime? dateFrom = null,
        [FromQuery] DateTime? dateTo = null)
    {
        try
        {
            if (page < 1 || pageSize < 1 || pageSize > 100)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Page must be >= 1 and pageSize must be between 1 and 100"
                });
            }

            var (logs, totalCount) = await _logRepository.GetLogsAsync(
                page, pageSize, inventoryId, productId, action, performedBy, dateFrom, dateTo);

            var logDtos = logs.Select(l => new InventoryLogDto
            {
                Id = l.Id,
                InventoryId = l.InventoryId,
                ProductId = l.ProductId,
                Action = l.Action,
                OldQuantity = l.OldQuantity,
                NewQuantity = l.NewQuantity,
                QuantityChange = l.QuantityChange,
                Reason = l.Reason,
                PerformedBy = l.PerformedBy,
                PerformedAt = l.PerformedAt
            }).ToList();

            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            return Ok(new PaginatedResponseDto<InventoryLogDto>
            {
                Data = logDtos,
                Pagination = new PaginationMetadata
                {
                    CurrentPage = page,
                    PageSize = pageSize,
                    TotalCount = totalCount,
                    TotalPages = totalPages
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting inventory logs");
            return StatusCode(500, new
            {
                success = false,
                message = "An error occurred while retrieving inventory logs",
                error = ex.Message
            });
        }
    }

}
