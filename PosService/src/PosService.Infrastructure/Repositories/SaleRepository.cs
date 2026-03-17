using PosService.Domain.Entities;
using PosService.Application.Interfaces;
using Microsoft.Extensions.Logging;
using PosService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace PosService.Infrastructure.Repositories;

public class SaleRepository : ISaleRepository
{
    private readonly PosDbContext _context;
    private readonly ILogger<SaleRepository> _logger;

    public SaleRepository(PosDbContext context, ILogger<SaleRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Sale> CreateAsync(Sale sale)
    {
        if (sale.Id == Guid.Empty)
        {
            sale.Id = Guid.NewGuid();
        }
        if (sale.CreatedAt == default)
        {
            sale.CreatedAt = DateTime.UtcNow;
        }
        
        if (string.IsNullOrEmpty(sale.SaleNumber))
        {
            sale.SaleNumber = await GenerateNextSaleNumberAsync();
        }

        await _context.Sales.AddAsync(sale);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Created and saved new sale with ID {SaleId} and Number {SaleNumber} to the database.", sale.Id, sale.SaleNumber);
        return sale;
    }

    public async Task<Sale?> GetByIdAsync(Guid id)
    {
        _logger.LogInformation("Fetching sale by ID from database: {SaleId}", id);
        var sale = await _context.Sales
            .Include(s => s.SaleItems)
            .Include(s => s.Payments)
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id);
        
        if (sale == null)
        {
            _logger.LogWarning("Sale with ID {SaleId} not found in database.", id);
        }
        return sale;
    }

    public async Task<(IEnumerable<Sale> Items, int TotalCount)> GetAllAsync(
        int page = 1,
        int pageSize = 10,
        Guid? storeId = null,
        Guid? cashierId = null,
        string? paymentMethod = null,
        string? status = null,
        DateTime? dateFrom = null,
        DateTime? dateTo = null)
    {
        var query = _context.Sales
            .Include(s => s.SaleItems)
            .Include(s => s.Payments)
            .AsNoTracking();

        if (storeId.HasValue)
            query = query.Where(s => s.StoreId == storeId.Value);
        if (cashierId.HasValue)
            query = query.Where(s => s.CashierId == cashierId.Value);
        if (!string.IsNullOrEmpty(paymentMethod))
            query = query.Where(s => s.PaymentMethod == paymentMethod);
        if (!string.IsNullOrEmpty(status))
            query = query.Where(s => s.Status == status);
        if (dateFrom.HasValue)
            query = query.Where(s => s.SaleDate >= dateFrom.Value);
        if (dateTo.HasValue)
            query = query.Where(s => s.SaleDate <= dateTo.Value);

        var totalCount = await query.CountAsync();

        var items = await query.OrderByDescending(s => s.SaleDate)
                               .Skip((page - 1) * pageSize)
                               .Take(pageSize)
                               .ToListAsync();

        return (items, totalCount);
    }

    public async Task<Sale> UpdateAsync(Sale sale)
    {
        sale.UpdatedAt = DateTime.UtcNow;
        _context.Sales.Update(sale);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Updated sale with ID {SaleId} in the database.", sale.Id);
        return sale;
    }
    
    private async Task<string> GenerateNextSaleNumberAsync()
    {
        var today = DateTime.UtcNow.ToString("yyyyMMdd");
        var count = await _context.Sales.CountAsync(s => s.SaleNumber.StartsWith($"SALE-{today}-"));
        return $"SALE-{today}-{count + 1:D4}";
    }
}
