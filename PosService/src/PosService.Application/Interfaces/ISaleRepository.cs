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

    // Cart Management
    Task<Sale> CreateCartAsync(Guid storeId, Guid cashierId, Guid? customerId, string paymentMethod, string? notes);
    Task<Sale?> GetCartByIdAsync(Guid cartId);
    Task<SaleItem> AddItemToCartAsync(Guid cartId, Guid productId, string productName, decimal unitPrice, int quantity);
    Task<SaleItem?> UpdateCartItemAsync(Guid cartId, Guid itemId, int quantity);
    Task<bool> RemoveCartItemAsync(Guid cartId, Guid itemId);
    Task<Sale?> RecalculateCartTotalsAsync(Guid cartId);
    Task<Sale?> CompleteCartAsync(Guid cartId);
}
