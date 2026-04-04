using Microsoft.EntityFrameworkCore;
using PosService.Application.DTOs;
using PosService.Application.Interfaces;
using PosService.Infrastructure.Data;

namespace PosService.Application.Services;

public class RevenueReportService : IRevenueReportService
{
    private readonly PosDbContext _dbContext;

    public RevenueReportService(PosDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<RevenueReportResponseDto> GetManagerRevenueAsync(
        Guid managerStoreId,
        RevenueReportRequestDto request,
        CancellationToken cancellationToken = default)
    {
        return GetRevenueAsync(managerStoreId, request, cancellationToken);
    }

    public Task<RevenueReportResponseDto> GetAdminRevenueAsync(
        RevenueReportRequestDto request,
        CancellationToken cancellationToken = default)
    {
        return GetRevenueAsync(request.StoreId, request, cancellationToken);
    }

    private async Task<RevenueReportResponseDto> GetRevenueAsync(
        Guid? storeId,
        RevenueReportRequestDto request,
        CancellationToken cancellationToken)
    {
        var (fromUtc, toUtcExclusive) = ResolveDateRange(request);

        var completedSales = _dbContext.Sales
            .AsNoTracking()
            .Where(s => s.Status == "COMPLETED" && s.PaymentStatus == "PAID")
            .Where(s => s.SaleDate >= fromUtc && s.SaleDate < toUtcExclusive);

        if (storeId.HasValue)
        {
            completedSales = completedSales.Where(s => s.StoreId == storeId.Value);
        }

        var totalOrders = await completedSales.CountAsync(cancellationToken);
        if (totalOrders == 0)
        {
            return new RevenueReportResponseDto();
        }

        var saleIds = completedSales.Select(s => s.Id);

        var saleItemsQuery = _dbContext.SaleItems
            .AsNoTracking()
            .Where(si => saleIds.Contains(si.SaleId));

        var totalRevenue = await saleItemsQuery
            .SumAsync(si => (decimal?)(si.Quantity * si.UnitPrice), cancellationToken) ?? 0m;

        var totalProductsSold = await saleItemsQuery
            .SumAsync(si => (int?)si.Quantity, cancellationToken) ?? 0;

        var topProducts = await saleItemsQuery
            .GroupBy(si => new { si.ProductId, si.ProductName })
            .Select(group => new TopRevenueProductDto
            {
                ProductId = group.Key.ProductId,
                ProductName = group.Key.ProductName,
                QuantitySold = group.Sum(x => x.Quantity),
                Revenue = group.Sum(x => x.Quantity * x.UnitPrice)
            })
            .OrderByDescending(x => x.QuantitySold)
            .ThenByDescending(x => x.Revenue)
            .Take(5)
            .ToListAsync(cancellationToken);

        return new RevenueReportResponseDto
        {
            TotalRevenue = totalRevenue,
            TotalOrders = totalOrders,
            TotalProductsSold = totalProductsSold,
            TopProducts = topProducts
        };
    }

    private static (DateTime FromUtc, DateTime ToUtcExclusive) ResolveDateRange(RevenueReportRequestDto request)
    {
        // If Period is specified, use it (takes precedence over FilterType)
        if (!string.IsNullOrWhiteSpace(request.Period))
        {
            var period = request.Period.Trim().ToUpperInvariant();
            var today = DateTime.UtcNow.Date;

            return period switch
            {
                "TODAY" => (today, today.AddDays(1)),
                "YESTERDAY" => (today.AddDays(-1), today),
                "THIS_MONTH" => (new DateTime(today.Year, today.Month, 1), today.AddMonths(1)),
                "LAST_7_DAYS" => (today.AddDays(-6), today.AddDays(1)),
                "CUSTOM" => ResolveCustomDateRange(request),
                _ => ResolveDateRangeByFilterType(request) // Fallback to FilterType if unknown period
            };
        }

        // Fallback to FilterType-based resolution
        return ResolveDateRangeByFilterType(request);
    }

    private static (DateTime FromUtc, DateTime ToUtcExclusive) ResolveDateRangeByFilterType(RevenueReportRequestDto request)
    {
        var filterType = (request.FilterType ?? "DAY").Trim().ToUpperInvariant();

        if (request.FromDate.HasValue && request.ToDate.HasValue)
        {
            var from = request.FromDate.Value.Date;
            var to = request.ToDate.Value.Date;

            if (from > to)
            {
                throw new ArgumentException("Invalid date range: fromDate must be earlier than or equal to toDate.");
            }

            return (from, to.AddDays(1));
        }

        if (filterType == "RANGE")
        {
            if (!request.FromDate.HasValue || !request.ToDate.HasValue)
            {
                throw new ArgumentException("fromDate and toDate are required when filterType is RANGE.");
            }

            var from = request.FromDate.Value.Date;
            var to = request.ToDate.Value.Date;

            if (from > to)
            {
                throw new ArgumentException("Invalid date range: fromDate must be earlier than or equal to toDate.");
            }

            return (from, to.AddDays(1));
        }

        var pivot = request.FromDate?.Date ?? DateTime.UtcNow.Date;

        return filterType switch
        {
            "DAY" => (pivot, pivot.AddDays(1)),
            "MONTH" => (new DateTime(pivot.Year, pivot.Month, 1), new DateTime(pivot.Year, pivot.Month, 1).AddMonths(1)),
            "YEAR" => (new DateTime(pivot.Year, 1, 1), new DateTime(pivot.Year, 1, 1).AddYears(1)),
            _ => throw new ArgumentException("filterType must be one of DAY, MONTH, YEAR, RANGE.")
        };
    }

    private static (DateTime FromUtc, DateTime ToUtcExclusive) ResolveCustomDateRange(RevenueReportRequestDto request)
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

        return (from, to.AddDays(1));
    }
}
