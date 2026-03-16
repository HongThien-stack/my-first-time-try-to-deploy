using PosService.Domain.Entities;

namespace PosService.Application.Interfaces;

public interface ISaleRepository
{
    Task<(IEnumerable<Sale> Items, int TotalCount)> GetAllAsync(
        int page = 1,
        int pageSize = 10,
        Guid? storeId = null,
        Guid? cashierId = null,
        string? paymentMethod = null,
        string? status = null,
        DateTime? dateFrom = null,
        DateTime? dateTo = null);
}

