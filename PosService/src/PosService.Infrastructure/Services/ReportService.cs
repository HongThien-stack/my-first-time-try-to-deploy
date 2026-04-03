using Microsoft.EntityFrameworkCore;
using PosService.Application.DTOs;
using PosService.Application.Interfaces;
using PosService.Application.Interfaces.Http;
using PosService.Infrastructure.Data;

namespace PosService.Application.Services;

public class ReportService : IReportService
{
    private const int MaxTopN = 100;
    private readonly PosDbContext _dbContext;
    private readonly IInventoryServiceClient _inventoryServiceClient;

    public ReportService(PosDbContext dbContext, IInventoryServiceClient inventoryServiceClient)
    {
        _dbContext = dbContext;
        _inventoryServiceClient = inventoryServiceClient;
    }

    public async Task<IReadOnlyList<RevenueTrendPointDto>> GetRevenueTrendAsync(
        RevenueTrendRequestDto request,
        Guid? storeId,
        CancellationToken cancellationToken = default)
    {
        var groupBy = NormalizeGroupBy(request.GroupBy);
        var (fromUtc, toUtcExclusive) = ResolveDateRange(request.FromDate, request.ToDate, groupBy);

        var query = BuildCompletedSalesQuery(fromUtc, toUtcExclusive, storeId)
            .SelectMany(s => s.SaleItems)
            .AsNoTracking();

        return groupBy switch
        {
            "DAY" => await BuildDailyTrendAsync(query, cancellationToken),
            "MONTH" => await BuildMonthlyTrendAsync(query, cancellationToken),
            _ => await BuildYearlyTrendAsync(query, cancellationToken)
        };
    }

    public async Task<IReadOnlyList<TopProductReportDto>> GetTopProductsAsync(
        TopProductsRequestDto request,
        Guid? storeId,
        CancellationToken cancellationToken = default)
    {
        var topN = request.TopN <= 0 ? 10 : Math.Min(request.TopN, MaxTopN);
        var (fromUtc, toUtcExclusive) = ResolveDateRange(request.FromDate, request.ToDate, "DAY");

        var query = BuildCompletedSalesQuery(fromUtc, toUtcExclusive, storeId)
            .SelectMany(s => s.SaleItems)
            .AsNoTracking();

        return await query
            .GroupBy(si => new { si.ProductId, si.ProductName })
            .Select(g => new TopProductReportDto
            {
                ProductId = g.Key.ProductId,
                ProductName = g.Key.ProductName,
                QuantitySold = g.Sum(x => x.Quantity),
                Revenue = g.Sum(x => x.Quantity * x.UnitPrice)
            })
            .OrderByDescending(x => x.QuantitySold)
            .ThenByDescending(x => x.Revenue)
            .Take(topN)
            .ToListAsync(cancellationToken);
    }

    public async Task<InventorySummaryDto> GetInventorySummaryAsync(
        Guid? storeId,
        int lowStockThreshold = 10,
        CancellationToken cancellationToken = default)
    {
        var productIds = await _dbContext.Sales
            .AsNoTracking()
            .Where(s => s.Status == "COMPLETED" && s.PaymentStatus == "PAID")
            .Where(s => !storeId.HasValue || s.StoreId == storeId.Value)
            .SelectMany(s => s.SaleItems.Select(si => si.ProductId))
            .Distinct()
            .ToListAsync(cancellationToken);

        if (productIds.Count == 0)
        {
            return new InventorySummaryDto();
        }

        var stockLevels = await _inventoryServiceClient.GetStockLevelsBatchAsync(productIds);

        var totalProducts = productIds.Count;
        var totalStock = stockLevels.Values.Where(v => v > 0).Sum();
        var outOfStock = productIds.Count(id => !stockLevels.TryGetValue(id, out var qty) || qty <= 0);
        var lowStock = productIds.Count(id => stockLevels.TryGetValue(id, out var qty) && qty > 0 && qty < lowStockThreshold);

        return new InventorySummaryDto
        {
            TotalProducts = totalProducts,
            TotalStock = totalStock,
            LowStock = lowStock,
            OutOfStock = outOfStock
        };
    }

    private IQueryable<Domain.Entities.Sale> BuildCompletedSalesQuery(DateTime fromUtc, DateTime toUtcExclusive, Guid? storeId)
    {
        var query = _dbContext.Sales
            .AsNoTracking()
            .Where(s => s.Status == "COMPLETED" && s.PaymentStatus == "PAID")
            .Where(s => s.SaleDate >= fromUtc && s.SaleDate < toUtcExclusive);

        if (storeId.HasValue)
        {
            query = query.Where(s => s.StoreId == storeId.Value);
        }

        return query;
    }

    private static string NormalizeGroupBy(string? groupBy)
    {
        var normalized = (groupBy ?? "DAY").Trim().ToUpperInvariant();
        if (normalized is "DAY" or "MONTH" or "YEAR")
        {
            return normalized;
        }

        throw new ArgumentException("groupBy must be one of DAY, MONTH, YEAR.");
    }

    private static (DateTime FromUtc, DateTime ToUtcExclusive) ResolveDateRange(DateTime? fromDate, DateTime? toDate, string groupBy)
    {
        if (fromDate.HasValue && toDate.HasValue)
        {
            var from = fromDate.Value.Date;
            var to = toDate.Value.Date;

            if (from > to)
            {
                throw new ArgumentException("Invalid date range: fromDate must be earlier than or equal to toDate.");
            }

            return (from, to.AddDays(1));
        }

        // Default window when missing parameters.
        var today = DateTime.UtcNow.Date;
        return groupBy switch
        {
            "DAY" => (today.AddDays(-29), today.AddDays(1)),
            "MONTH" => (new DateTime(today.Year, 1, 1), today.AddDays(1)),
            "YEAR" => (new DateTime(today.Year - 4, 1, 1), new DateTime(today.Year + 1, 1, 1)),
            _ => (today.AddDays(-29), today.AddDays(1))
        };
    }

    private static async Task<IReadOnlyList<RevenueTrendPointDto>> BuildDailyTrendAsync(
        IQueryable<Domain.Entities.SaleItem> query,
        CancellationToken cancellationToken)
    {
        var rows = await query
            .GroupBy(si => new
            {
                si.Sale.SaleDate.Year,
                si.Sale.SaleDate.Month,
                si.Sale.SaleDate.Day
            })
            .Select(g => new
            {
                g.Key.Year,
                g.Key.Month,
                g.Key.Day,
                Revenue = g.Sum(x => x.Quantity * x.UnitPrice)
            })
            .OrderBy(x => x.Year)
            .ThenBy(x => x.Month)
            .ThenBy(x => x.Day)
            .ToListAsync(cancellationToken);

        return rows
            .Select(x => new RevenueTrendPointDto
            {
                Time = $"{x.Year:D4}-{x.Month:D2}-{x.Day:D2}",
                Revenue = x.Revenue
            })
            .ToList();
    }

    private static async Task<IReadOnlyList<RevenueTrendPointDto>> BuildMonthlyTrendAsync(
        IQueryable<Domain.Entities.SaleItem> query,
        CancellationToken cancellationToken)
    {
        var rows = await query
            .GroupBy(si => new { si.Sale.SaleDate.Year, si.Sale.SaleDate.Month })
            .Select(g => new
            {
                g.Key.Year,
                g.Key.Month,
                Revenue = g.Sum(x => x.Quantity * x.UnitPrice)
            })
            .OrderBy(x => x.Year)
            .ThenBy(x => x.Month)
            .ToListAsync(cancellationToken);

        return rows
            .Select(x => new RevenueTrendPointDto
            {
                Time = $"{x.Year:D4}-{x.Month:D2}",
                Revenue = x.Revenue
            })
            .ToList();
    }

    private static async Task<IReadOnlyList<RevenueTrendPointDto>> BuildYearlyTrendAsync(
        IQueryable<Domain.Entities.SaleItem> query,
        CancellationToken cancellationToken)
    {
        var rows = await query
            .GroupBy(si => si.Sale.SaleDate.Year)
            .Select(g => new
            {
                Year = g.Key,
                Revenue = g.Sum(x => x.Quantity * x.UnitPrice)
            })
            .OrderBy(x => x.Year)
            .ToListAsync(cancellationToken);

        return rows
            .Select(x => new RevenueTrendPointDto
            {
                Time = x.Year.ToString("D4"),
                Revenue = x.Revenue
            })
            .ToList();
    }
}
