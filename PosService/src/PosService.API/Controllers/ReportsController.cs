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

    public ReportsController(IRevenueReportService revenueReportService)
    {
        _revenueReportService = revenueReportService;
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
