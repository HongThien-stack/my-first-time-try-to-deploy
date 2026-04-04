using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PosService.Application.DTOs;
using PosService.Application.Interfaces;

namespace PosService.API.Controllers;

[ApiController]
[Route("api/reports")]
public class ReportsController : ControllerBase
{
    private static readonly string[] StoreClaimKeys =
    {
        "store_id",
        "storeId",
        "workplace_id",
        "workplaceId"
    };

    private static readonly string[] StoreNameClaimKeys =
    {
        "store_name",
        "storeName",
        "workplace_name",
        "workplaceName"
    };

    private readonly IRevenueReportService _revenueReportService;
    private readonly IReportService _reportService;

    public ReportsController(IRevenueReportService revenueReportService, IReportService reportService)
    {
        _revenueReportService = revenueReportService;
        _reportService = reportService;
    }

    [HttpGet("manager/revenue")]
    [Authorize(Roles = "Manager,Store Manager")]
    public async Task<IActionResult> GetManagerRevenue([FromQuery] RevenueReportRequestDto request, CancellationToken cancellationToken)
    {
        // Manager endpoint must never accept a storeId from query.
        // Store scope is enforced only from JWT claims.
        if (request.StoreId.HasValue)
        {
            return Forbid();
        }

        var workplaceType = User.FindFirst("workplace_type")?.Value;
        if (!string.Equals(workplaceType, "STORE", StringComparison.OrdinalIgnoreCase))
        {
            return Forbid();
        }

        var managerStoreId = ExtractStoreIdFromClaims(User);
        if (!managerStoreId.HasValue)
        {
            return Forbid();
        }

        request.StoreId = managerStoreId.Value;

        try
        {
            var result = await _revenueReportService.GetManagerRevenueAsync(managerStoreId.Value, request, cancellationToken);
            result.StoreId = managerStoreId.Value;
            result.StoreName = ExtractStoreNameFromClaims(User);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("admin/revenue")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAdminRevenue([FromQuery] RevenueReportRequestDto request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _revenueReportService.GetAdminRevenueAsync(request, cancellationToken);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("revenue-trend")]
    [Authorize(Roles = "Admin,Manager,Store Manager")]
    public async Task<IActionResult> GetRevenueTrend([FromQuery] RevenueTrendRequestDto request, CancellationToken cancellationToken)
    {
        try
        {
            var scopedStoreId = ResolveScopedStoreId();
            var result = await _reportService.GetRevenueTrendAsync(request, scopedStoreId, cancellationToken);
            return Ok(result);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("top-products")]
    [Authorize(Roles = "Admin,Manager,Store Manager")]
    public async Task<IActionResult> GetTopProducts([FromQuery] TopProductsRequestDto request, CancellationToken cancellationToken)
    {
        try
        {
            if (!User.IsInRole("Admin") && request.StoreId.HasValue)
            {
                return Forbid();
            }

            var scopedStoreId = ResolveScopedStoreId();
            var effectiveStoreId = scopedStoreId ?? request.StoreId;
            var result = await _reportService.GetTopProductsAsync(request, effectiveStoreId, cancellationToken);
            return Ok(result);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("admin/top-products-trend")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAdminTopProductsTrend([FromQuery] TopProductsRequestDto request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _reportService.GetAdminTopProductsTrendAsync(request, cancellationToken);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("inventory-summary")]
    [Authorize(Roles = "Admin,Manager,Store Manager")]
    public async Task<IActionResult> GetInventorySummary([FromQuery] int lowStockThreshold = 10, CancellationToken cancellationToken = default)
    {
        try
        {
            var scopedStoreId = ResolveScopedStoreId();
            var result = await _reportService.GetInventorySummaryAsync(scopedStoreId, lowStockThreshold, cancellationToken);
            return Ok(result);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    [HttpGet("manager/dashboard")]
    [Authorize(Roles = "Manager,Store Manager")]
    public async Task<IActionResult> GetManagerDashboard([FromQuery] StoreDashboardRequestDto request, CancellationToken cancellationToken)
    {
        if (request == null)
        {
            return BadRequest(new { message = "Request is required." });
        }

        if (request.TopN <= 0)
        {
            request.TopN = 5;
        }

        if (request.LowStockThreshold <= 0)
        {
            request.LowStockThreshold = 10;
        }

        var workplaceType = User.FindFirst("workplace_type")?.Value;
        if (!string.Equals(workplaceType, "STORE", StringComparison.OrdinalIgnoreCase))
        {
            return Forbid();
        }

        var managerStoreId = ExtractStoreIdFromClaims(User);
        if (!managerStoreId.HasValue)
        {
            return Forbid();
        }

        try
        {
            var (fromDate, toDate, normalizedPeriod) = ResolveDashboardDateRange(request);

            var revenueRequest = new RevenueReportRequestDto
            {
                StoreId = managerStoreId.Value,
                FilterType = "RANGE",
                FromDate = fromDate,
                ToDate = toDate
            };

            var trendRequest = new RevenueTrendRequestDto
            {
                FromDate = fromDate,
                ToDate = toDate,
                GroupBy = string.IsNullOrWhiteSpace(request.GroupBy) ? "DAY" : request.GroupBy
            };

            var topProductsRequest = new TopProductsRequestDto
            {
                FromDate = fromDate,
                ToDate = toDate,
                TopN = request.TopN
            };

            var revenue = await _revenueReportService.GetManagerRevenueAsync(managerStoreId.Value, revenueRequest, cancellationToken);
            revenue.StoreId = managerStoreId.Value;
            revenue.StoreName = ExtractStoreNameFromClaims(User);
            var revenueTrend = await _reportService.GetRevenueTrendAsync(trendRequest, managerStoreId, cancellationToken);
            var topProducts = await _reportService.GetTopProductsAsync(topProductsRequest, managerStoreId, cancellationToken);
            var inventorySummary = await _reportService.GetInventorySummaryAsync(managerStoreId, request.LowStockThreshold, cancellationToken);

            return Ok(new StoreDashboardResponseDto
            {
                StoreId = managerStoreId.Value,
                StoreName = ExtractStoreNameFromClaims(User),
                Period = normalizedPeriod,
                FromDate = fromDate,
                ToDate = toDate,
                Revenue = revenue,
                RevenueTrend = revenueTrend,
                TopProducts = topProducts,
                InventorySummary = inventorySummary
            });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    private Guid? ResolveScopedStoreId()
    {
        if (User.IsInRole("Admin"))
        {
            return null;
        }

        var workplaceType = User.FindFirst("workplace_type")?.Value;
        if (!string.Equals(workplaceType, "STORE", StringComparison.OrdinalIgnoreCase))
        {
            throw new UnauthorizedAccessException("Only STORE-scoped manager can access this report.");
        }

        var managerStoreId = ExtractStoreIdFromClaims(User);
        if (!managerStoreId.HasValue)
        {
            throw new UnauthorizedAccessException("Store scope is required for non-admin users.");
        }

        return managerStoreId.Value;
    }

    private static (DateTime FromDate, DateTime ToDate, string Period) ResolveDashboardDateRange(StoreDashboardRequestDto request)
    {
        var period = (request.Period ?? "LAST_7_DAYS").Trim().ToUpperInvariant();
        var today = DateTime.UtcNow.Date;

        return period switch
        {
            "TODAY" => (today, today, "TODAY"),
            "YESTERDAY" => (today.AddDays(-1), today.AddDays(-1), "YESTERDAY"),
            "THIS_MONTH" => (new DateTime(today.Year, today.Month, 1), today, "THIS_MONTH"),
            "CUSTOM" => ResolveCustomDateRange(request),
            _ => (today.AddDays(-6), today, "LAST_7_DAYS")
        };
    }

    private static (DateTime FromDate, DateTime ToDate, string Period) ResolveCustomDateRange(StoreDashboardRequestDto request)
    {
        if (!request.FromDate.HasValue || !request.ToDate.HasValue)
        {
            throw new ArgumentException("fromDate and toDate are required when period is CUSTOM.");
        }

        var from = request.FromDate.Value.Date;
        var to = request.ToDate.Value.Date;

        if (from > to)
        {
            throw new ArgumentException("Invalid date range: fromDate must be earlier than or equal to toDate.");
        }

        return (from, to, "CUSTOM");
    }

    private static Guid? ExtractStoreIdFromClaims(ClaimsPrincipal user)
    {
        foreach (var claimKey in StoreClaimKeys)
        {
            var claimValue = user.FindFirst(claimKey)?.Value;
            if (Guid.TryParse(claimValue, out var parsedStoreId))
            {
                return parsedStoreId;
            }
        }

        return null;
    }

    private static string ExtractStoreNameFromClaims(ClaimsPrincipal user)
    {
        foreach (var claimKey in StoreNameClaimKeys)
        {
            var claimValue = user.FindFirst(claimKey)?.Value;
            if (!string.IsNullOrWhiteSpace(claimValue))
            {
                return claimValue;
            }
        }

        return "UNKNOWN_STORE";
    }
}
