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
            var scopedStoreId = ResolveScopedStoreId();
            var result = await _reportService.GetTopProductsAsync(request, scopedStoreId, cancellationToken);
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

    private Guid? ResolveScopedStoreId()
    {
        if (User.IsInRole("Admin"))
        {
            return null;
        }

        var managerStoreId = ExtractStoreIdFromClaims(User);
        if (!managerStoreId.HasValue)
        {
            throw new UnauthorizedAccessException("Store scope is required for non-admin users.");
        }

        return managerStoreId.Value;
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
}
