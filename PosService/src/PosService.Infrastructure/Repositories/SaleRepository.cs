using PosService.Domain.Entities;
using PosService.Application.Interfaces;

namespace PosService.Infrastructure.Repositories;

public class SaleRepository : ISaleRepository
{
    private readonly List<Sale> _sales;
    private readonly List<(Guid ProductId, string ProductName, string Sku, string Barcode, decimal UnitPrice)> _mockProducts;
    private int _saleCounter = 11; // Starting from SALE-2024-011

    public SaleRepository()
    {
        // Mock product database (simulating ProductDB)
        _mockProducts = new List<(Guid, string, string, string, decimal)>
        {
            (Guid.Parse("F0000001-0001-0001-0001-000000000001"), "Rau Muống", "RAU-001", "8934560001234", 20000m),
            (Guid.Parse("F0000001-0001-0001-0001-000000000003"), "Cam Sành", "TC-001", "8934560002234", 35000m),
            (Guid.Parse("F0000001-0001-0001-0001-000000000004"), "Táo Envy", "TC-002", "8934560002241", 150000m),
            (Guid.Parse("F0000001-0001-0001-0001-000000000005"), "Sữa Tươi Vinamilk 100%", "SUA-001", "8934560003234", 38000m),
            (Guid.Parse("F0000001-0001-0001-0001-000000000006"), "Sữa TH True Milk", "SUA-002", "8934560003241", 42000m),
            (Guid.Parse("F0000001-0001-0001-0001-000000000007"), "Gạo ST25", "GAO-001", "8934560004234", 180000m),
            (Guid.Parse("F0000001-0001-0001-0001-000000000008"), "Gạo Jasmine", "GAO-002", "8934560004241", 140000m),
        };

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
            },
            
            // ===== SAMPLE CARTS (PENDING) for Testing =====
            new Sale
            {
                Id = Guid.Parse("AA100001-0001-0001-0001-000000000001"),
                SaleNumber = "CART-2026-001",
                StoreId = store1,
                CashierId = cashier1,
                CustomerId = customer1,
                SaleDate = DateTime.UtcNow,
                Subtotal = 76000m,
                TaxAmount = 0m,
                DiscountAmount = 0m,
                TotalAmount = 76000m,
                PaymentMethod = "PENDING",
                PaymentStatus = "PENDING",
                Status = "PENDING",
                Notes = "Test cart with items",
                CreatedAt = DateTime.UtcNow.AddMinutes(-30),
                UpdatedAt = DateTime.UtcNow.AddMinutes(-10),
                SaleItems = new List<SaleItem>
                {
                    new SaleItem
                    {
                        Id = Guid.Parse("11110001-0001-0001-0001-000000000001"),
                        SaleId = Guid.Parse("AA100001-0001-0001-0001-000000000001"),
                        ProductId = Guid.Parse("F0000001-0001-0001-0001-000000000005"),
                        ProductName = "Sữa Tươi Vinamilk 100%",
                        ProductSku = "SUA-001",
                        Quantity = 2,
                        UnitPrice = 38000m,
                        LineDiscount = 0m,
                        LineTax = 0m,
                        LineTotal = 76000m
                    }
                }
            },
            new Sale
            {
                Id = Guid.Parse("AA100002-0001-0001-0001-000000000001"),
                SaleNumber = "CART-2026-002",
                StoreId = store1,
                CashierId = cashier1,
                CustomerId = null,
                SaleDate = DateTime.UtcNow,
                Subtotal = 0m,
                TaxAmount = 0m,
                DiscountAmount = 0m,
                TotalAmount = 0m,
                PaymentMethod = "PENDING",
                PaymentStatus = "PENDING",
                Status = "PENDING",
                Notes = "Empty cart",
                CreatedAt = DateTime.UtcNow.AddMinutes(-5),
                SaleItems = new List<SaleItem>()
            },
            new Sale
            {
                Id = Guid.Parse("AA100003-0001-0001-0001-000000000001"),
                SaleNumber = "CART-2026-003",
                StoreId = store2,
                CashierId = cashier2,
                CustomerId = customer1,
                SaleDate = DateTime.UtcNow,
                Subtotal = 395000m,
                TaxAmount = 0m,
                DiscountAmount = 0m,
                TotalAmount = 395000m,
                PaymentMethod = "PENDING",
                PaymentStatus = "PENDING",
                Status = "PENDING",
                Notes = "Cart with multiple items",
                CreatedAt = DateTime.UtcNow.AddMinutes(-45),
                UpdatedAt = DateTime.UtcNow.AddMinutes(-20),
                SaleItems = new List<SaleItem>
                {
                    new SaleItem
                    {
                        Id = Guid.Parse("11110003-0001-0001-0001-000000000001"),
                        SaleId = Guid.Parse("AA100003-0001-0001-0001-000000000001"),
                        ProductId = Guid.Parse("F0000001-0001-0001-0001-000000000007"),
                        ProductName = "Gạo ST25",
                        ProductSku = "GAO-001",
                        Quantity = 2,
                        UnitPrice = 180000m,
                        LineDiscount = 0m,
                        LineTax = 0m,
                        LineTotal = 360000m
                    },
                    new SaleItem
                    {
                        Id = Guid.Parse("11110003-0001-0001-0001-000000000002"),
                        SaleId = Guid.Parse("AA100003-0001-0001-0001-000000000001"),
                        ProductId = Guid.Parse("F0000001-0001-0001-0001-000000000003"),
                        ProductName = "Cam Sành",
                        ProductSku = "TC-001",
                        Quantity = 1,
                        UnitPrice = 35000m,
                        LineDiscount = 0m,
                        LineTax = 0m,
                        LineTotal = 35000m
                    }
                }
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

    // =====================================================
    // Cart Management Methods
    // =====================================================

    public Task<Sale> CreateCartAsync(Guid storeId, Guid cashierId, Guid? customerId, string? notes)
    {
        var saleNumber = $"SALE-2026-{_saleCounter++:D3}";
        
        var cart = new Sale
        {
            Id = Guid.NewGuid(),
            SaleNumber = saleNumber,
            StoreId = storeId,
            CashierId = cashierId,
            CustomerId = customerId,
            SaleDate = DateTime.UtcNow,
            Subtotal = 0m,
            TaxAmount = 0m,
            DiscountAmount = 0m,
            TotalAmount = 0m,
            PaymentMethod = "PENDING",
            PaymentStatus = "PENDING",
            Status = "PENDING", // Cart is a PENDING sale
            Notes = notes,
            CreatedAt = DateTime.UtcNow,
            SaleItems = new List<SaleItem>()
        };

        _sales.Add(cart);
        return Task.FromResult(cart);
    }

    public Task<Sale?> GetCartByIdAsync(Guid cartId)
    {
        var cart = _sales.FirstOrDefault(s => s.Id == cartId && s.Status == "PENDING");
        return Task.FromResult(cart);
    }

    public async Task<SaleItem> AddItemToCartAsync(Guid cartId, string barcode, int quantity)
    {
        var cart = await GetCartByIdAsync(cartId);
        if (cart == null)
            throw new Exception("Cart not found or already completed");

        // Get product info
        var productInfo = await GetProductByBarcodeAsync(barcode);
        if (productInfo == null)
            throw new Exception($"Product with barcode {barcode} not found");

        // Check if product already exists in cart
        var existingItem = cart.SaleItems.FirstOrDefault(i => i.ProductId == productInfo.Value.ProductId);
        
        if (existingItem != null)
        {
            // Increase quantity
            existingItem.Quantity += quantity;
            existingItem.LineTotal = existingItem.Quantity * existingItem.UnitPrice - existingItem.LineDiscount;
        }
        else
        {
            // Add new item
            existingItem = new SaleItem
            {
                Id = Guid.NewGuid(),
                SaleId = cartId,
                ProductId = productInfo.Value.ProductId,
                ProductName = productInfo.Value.ProductName,
                ProductSku = productInfo.Value.Sku,
                Quantity = quantity,
                UnitPrice = productInfo.Value.UnitPrice,
                LineDiscount = 0m,
                LineTax = 0m,
                LineTotal = quantity * productInfo.Value.UnitPrice
            };
            cart.SaleItems.Add(existingItem);
        }

        // Recalculate totals
        await RecalculateCartTotalsAsync(cartId);

        return existingItem;
    }

    public async Task<SaleItem?> UpdateCartItemAsync(Guid cartId, Guid itemId, int quantity)
    {
        var cart = await GetCartByIdAsync(cartId);
        if (cart == null)
            return null;

        var item = cart.SaleItems.FirstOrDefault(i => i.Id == itemId);
        if (item == null)
            return null;

        if (quantity <= 0)
        {
            // If quantity is 0 or negative, remove the item
            cart.SaleItems.Remove(item);
        }
        else
        {
            item.Quantity = quantity;
            item.LineTotal = quantity * item.UnitPrice - item.LineDiscount;
        }

        // Recalculate totals
        await RecalculateCartTotalsAsync(cartId);

        return quantity > 0 ? item : null;
    }

    public async Task<bool> RemoveCartItemAsync(Guid cartId, Guid itemId)
    {
        var cart = await GetCartByIdAsync(cartId);
        if (cart == null)
            return false;

        var item = cart.SaleItems.FirstOrDefault(i => i.Id == itemId);
        if (item == null)
            return false;

        cart.SaleItems.Remove(item);

        // Recalculate totals
        await RecalculateCartTotalsAsync(cartId);

        return true;
    }

    public Task<Sale?> RecalculateCartTotalsAsync(Guid cartId)
    {
        var cart = _sales.FirstOrDefault(s => s.Id == cartId);
        if (cart == null)
            return Task.FromResult<Sale?>(null);

        // Calculate subtotal from all items
        cart.Subtotal = cart.SaleItems.Sum(i => i.LineTotal);
        
        // Calculate tax (10% of subtotal for this example)
        cart.TaxAmount = 0m; // No tax for now, can be customized
        
        // Total = Subtotal - Discount + Tax
        cart.TotalAmount = cart.Subtotal - cart.DiscountAmount + cart.TaxAmount;
        
        cart.UpdatedAt = DateTime.UtcNow;

        return Task.FromResult<Sale?>(cart);
    }

    public Task<(Guid ProductId, string ProductName, string Sku, string? Barcode, decimal UnitPrice)?> GetProductByBarcodeAsync(string barcode)
    {
        var product = _mockProducts.FirstOrDefault(p => p.Barcode == barcode);
        
        if (product == default)
            return Task.FromResult<(Guid, string, string, string?, decimal)?>(null);

        return Task.FromResult<(Guid, string, string, string?, decimal)?>((
            product.ProductId,
            product.ProductName,
            product.Sku,
            product.Barcode,
            product.UnitPrice
        ));
    }
}
