using PosService.Domain.Entities;
using PosService.Application.Interfaces;

namespace PosService.Infrastructure.Repositories;

public class SaleRepository : ISaleRepository
{
    private readonly List<Sale> _sales;

    public SaleRepository()
    {
        // Sample data for testing
        var store1 = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var store2 = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var cashier1 = Guid.Parse("33333333-3333-3333-3333-333333333333");
        var cashier2 = Guid.Parse("44444444-4444-4444-4444-444444444444");
        var customer1 = Guid.Parse("55555555-5555-5555-5555-555555555555");

        _sales = new List<Sale>
        {
            new Sale
            {
                Id = Guid.NewGuid(),
                SaleNumber = "SALE-2024-001",
                StoreId = store1,
                CashierId = cashier1,
                CustomerId = customer1,
                SaleDate = DateTime.UtcNow.AddDays(-5),
                Subtotal = 100.00m,
                TaxAmount = 10.00m,
                DiscountAmount = 5.00m,
                TotalAmount = 105.00m,
                PaymentMethod = "CASH",
                PaymentStatus = "COMPLETED",
                Status = "COMPLETED",
                PointsUsed = 0,
                PointsEarned = 10,
                CreatedAt = DateTime.UtcNow.AddDays(-5),
                SaleItems = new List<SaleItem>()
            },
            new Sale
            {
                Id = Guid.NewGuid(),
                SaleNumber = "SALE-2024-002",
                StoreId = store1,
                CashierId = cashier1,
                CustomerId = null,
                SaleDate = DateTime.UtcNow.AddDays(-4),
                Subtotal = 250.00m,
                TaxAmount = 25.00m,
                DiscountAmount = 0.00m,
                TotalAmount = 275.00m,
                PaymentMethod = "CARD",
                PaymentStatus = "COMPLETED",
                Status = "COMPLETED",
                PointsUsed = 0,
                PointsEarned = 27,
                CreatedAt = DateTime.UtcNow.AddDays(-4),
                SaleItems = new List<SaleItem>()
            },
            new Sale
            {
                Id = Guid.NewGuid(),
                SaleNumber = "SALE-2024-003",
                StoreId = store2,
                CashierId = cashier2,
                CustomerId = customer1,
                SaleDate = DateTime.UtcNow.AddDays(-3),
                Subtotal = 450.00m,
                TaxAmount = 45.00m,
                DiscountAmount = 50.00m,
                TotalAmount = 445.00m,
                PaymentMethod = "VNPAY",
                PaymentStatus = "COMPLETED",
                Status = "COMPLETED",
                PromotionId = Guid.NewGuid(),
                VoucherCode = "DISCOUNT50",
                PointsUsed = 100,
                PointsEarned = 44,
                CreatedAt = DateTime.UtcNow.AddDays(-3),
                SaleItems = new List<SaleItem>()
            },
            new Sale
            {
                Id = Guid.NewGuid(),
                SaleNumber = "SALE-2024-004",
                StoreId = store1,
                CashierId = cashier2,
                CustomerId = null,
                SaleDate = DateTime.UtcNow.AddDays(-2),
                Subtotal = 75.00m,
                TaxAmount = 7.50m,
                DiscountAmount = 0.00m,
                TotalAmount = 82.50m,
                PaymentMethod = "MOMO",
                PaymentStatus = "COMPLETED",
                Status = "COMPLETED",
                PointsUsed = 0,
                PointsEarned = 8,
                CreatedAt = DateTime.UtcNow.AddDays(-2),
                SaleItems = new List<SaleItem>()
            },
            new Sale
            {
                Id = Guid.NewGuid(),
                SaleNumber = "SALE-2024-005",
                StoreId = store2,
                CashierId = cashier1,
                CustomerId = customer1,
                SaleDate = DateTime.UtcNow.AddDays(-1),
                Subtotal = 300.00m,
                TaxAmount = 30.00m,
                DiscountAmount = 15.00m,
                TotalAmount = 315.00m,
                PaymentMethod = "CASH",
                PaymentStatus = "COMPLETED",
                Status = "COMPLETED",
                PointsUsed = 50,
                PointsEarned = 31,
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                SaleItems = new List<SaleItem>()
            },
            new Sale
            {
                Id = Guid.NewGuid(),
                SaleNumber = "SALE-2024-006",
                StoreId = store1,
                CashierId = cashier1,
                CustomerId = null,
                SaleDate = DateTime.UtcNow,
                Subtotal = 150.00m,
                TaxAmount = 15.00m,
                DiscountAmount = 0.00m,
                TotalAmount = 165.00m,
                PaymentMethod = "CARD",
                PaymentStatus = "PENDING",
                Status = "PENDING",
                PointsUsed = 0,
                PointsEarned = 0,
                CreatedAt = DateTime.UtcNow,
                SaleItems = new List<SaleItem>()
            },
            new Sale
            {
                Id = Guid.NewGuid(),
                SaleNumber = "SALE-2024-007",
                StoreId = store2,
                CashierId = cashier2,
                CustomerId = customer1,
                SaleDate = DateTime.UtcNow.AddHours(-6),
                Subtotal = 500.00m,
                TaxAmount = 50.00m,
                DiscountAmount = 100.00m,
                TotalAmount = 450.00m,
                PaymentMethod = "VNPAY",
                PaymentStatus = "COMPLETED",
                Status = "COMPLETED",
                PromotionId = Guid.NewGuid(),
                VoucherCode = "SAVE100",
                PointsUsed = 0,
                PointsEarned = 45,
                CreatedAt = DateTime.UtcNow.AddHours(-6),
                SaleItems = new List<SaleItem>()
            },
            new Sale
            {
                Id = Guid.NewGuid(),
                SaleNumber = "SALE-2024-008",
                StoreId = store1,
                CashierId = cashier1,
                CustomerId = null,
                SaleDate = DateTime.UtcNow.AddHours(-2),
                Subtotal = 80.00m,
                TaxAmount = 8.00m,
                DiscountAmount = 0.00m,
                TotalAmount = 88.00m,
                PaymentMethod = "CASH",
                PaymentStatus = "COMPLETED",
                Status = "COMPLETED",
                PointsUsed = 0,
                PointsEarned = 8,
                CreatedAt = DateTime.UtcNow.AddHours(-2),
                SaleItems = new List<SaleItem>()
            },
            new Sale
            {
                Id = Guid.NewGuid(),
                SaleNumber = "SALE-2024-009",
                StoreId = store2,
                CashierId = cashier2,
                CustomerId = customer1,
                SaleDate = DateTime.UtcNow.AddDays(-7),
                Subtotal = 1200.00m,
                TaxAmount = 120.00m,
                DiscountAmount = 200.00m,
                TotalAmount = 1120.00m,
                PaymentMethod = "CARD",
                PaymentStatus = "COMPLETED",
                Status = "COMPLETED",
                PromotionId = Guid.NewGuid(),
                VoucherCode = "BIG200",
                PointsUsed = 500,
                PointsEarned = 112,
                CreatedAt = DateTime.UtcNow.AddDays(-7),
                SaleItems = new List<SaleItem>()
            },
            new Sale
            {
                Id = Guid.NewGuid(),
                SaleNumber = "SALE-2024-010",
                StoreId = store1,
                CashierId = cashier2,
                CustomerId = null,
                SaleDate = DateTime.UtcNow.AddDays(-10),
                Subtotal = 35.00m,
                TaxAmount = 3.50m,
                DiscountAmount = 0.00m,
                TotalAmount = 38.50m,
                PaymentMethod = "MOMO",
                PaymentStatus = "COMPLETED",
                Status = "COMPLETED",
                PointsUsed = 0,
                PointsEarned = 3,
                CreatedAt = DateTime.UtcNow.AddDays(-10),
                SaleItems = new List<SaleItem>()
            }
        };
    }

    public Task<(IEnumerable<Sale> Items, int TotalCount)> GetAllAsync(
        int page = 1,
        int pageSize = 10,
        Guid? storeId = null,
        Guid? cashierId = null,
        string? paymentMethod = null,
        string? status = null,
        DateTime? dateFrom = null,
        DateTime? dateTo = null)
    {
        var query = _sales.AsQueryable();

        // Apply filters
        if (storeId.HasValue)
            query = query.Where(s => s.StoreId == storeId.Value);

        if (cashierId.HasValue)
            query = query.Where(s => s.CashierId == cashierId.Value);

        if (!string.IsNullOrWhiteSpace(paymentMethod))
            query = query.Where(s => s.PaymentMethod.Equals(paymentMethod, StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(s => s.Status.Equals(status, StringComparison.OrdinalIgnoreCase));

        if (dateFrom.HasValue)
            query = query.Where(s => s.SaleDate >= dateFrom.Value);

        if (dateTo.HasValue)
            query = query.Where(s => s.SaleDate <= dateTo.Value);

        // Get total count before pagination
        var totalCount = query.Count();

        // Apply pagination and order by date descending
        var items = query
            .OrderByDescending(s => s.SaleDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return Task.FromResult((items.AsEnumerable(), totalCount));
    }
}
