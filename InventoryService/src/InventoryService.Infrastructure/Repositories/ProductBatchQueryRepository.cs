using InventoryService.Application.DTOs;
using InventoryService.Application.Interfaces;
using InventoryService.Infrastructure.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace InventoryService.Infrastructure.Repositories;

public class ProductBatchQueryRepository : IProductBatchQueryRepository
{
    private readonly InventoryDbContext _context;

    public ProductBatchQueryRepository(InventoryDbContext context)
    {
        _context = context;
    }

    public async Task<BatchDetailDto?> GetBatchDetailByIdAsync(Guid id)
    {
        const string sql = @"
SELECT
    b.id AS BatchId,
    b.product_id AS ProductId,
    p.name AS ProductName,
    p.unit AS Unit,
    CAST(b.manufacturing_date AS DATETIME2) AS ManufacturingDate,
    CAST(b.expiry_date AS DATETIME2) AS ExpiryDate,
    p.shelf_life_days AS ShelfLifeDays,
    p.is_perishable AS IsPerishable,
    CAST(b.quantity AS decimal(18,2)) AS Quantity,
    b.status AS Status,
    DATEDIFF(day, GETUTCDATE(), b.expiry_date) AS RemainingDays,
    CASE
        WHEN b.expiry_date < GETUTCDATE() THEN 'EXPIRED'
        WHEN p.is_perishable = 0 AND DATEDIFF(day, GETUTCDATE(), b.expiry_date) <= 30 THEN 'EXPIRING_SOON'
        WHEN p.is_perishable = 1 AND p.shelf_life_days <= 3 AND DATEDIFF(day, GETUTCDATE(), b.expiry_date) <= 1 THEN 'EXPIRING_SOON'
        WHEN p.is_perishable = 1 AND p.shelf_life_days <= 7 AND DATEDIFF(day, GETUTCDATE(), b.expiry_date) <= 2 THEN 'EXPIRING_SOON'
        WHEN p.is_perishable = 1 AND p.shelf_life_days > 7 AND DATEDIFF(day, GETUTCDATE(), b.expiry_date) <= 3 THEN 'EXPIRING_SOON'
        ELSE 'VALID'
    END AS ExpiryState
FROM InventoryDB.dbo.product_batches b
LEFT JOIN ProductDB.dbo.products p ON b.product_id = p.id
WHERE b.id = @BatchId";

        var batchIdParameter = new SqlParameter("@BatchId", id);

        return await _context.Database
            .SqlQueryRaw<BatchDetailDto>(sql, batchIdParameter)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<ExpiringSoonBatchDto>> GetExpiringSoonBatchesAsync()
    {
        const string sql = @"
WITH BatchComputed AS
(
    SELECT
        b.id AS BatchId,
        b.product_id AS ProductId,
        p.name AS ProductName,
        CAST(b.expiry_date AS DATETIME2) AS ExpiryDate,
        DATEDIFF(day, GETUTCDATE(), b.expiry_date) AS RemainingDays,
        CAST(b.quantity AS decimal(18,2)) AS Quantity,
        p.is_perishable AS IsPerishable,
        CASE
            WHEN b.expiry_date < GETUTCDATE() THEN 'EXPIRED'
            WHEN p.is_perishable = 0 AND DATEDIFF(day, GETUTCDATE(), b.expiry_date) <= 30 THEN 'EXPIRING_SOON'
            WHEN p.is_perishable = 1 AND p.shelf_life_days <= 3 AND DATEDIFF(day, GETUTCDATE(), b.expiry_date) <= 1 THEN 'EXPIRING_SOON'
            WHEN p.is_perishable = 1 AND p.shelf_life_days <= 7 AND DATEDIFF(day, GETUTCDATE(), b.expiry_date) <= 2 THEN 'EXPIRING_SOON'
            WHEN p.is_perishable = 1 AND p.shelf_life_days > 7 AND DATEDIFF(day, GETUTCDATE(), b.expiry_date) <= 3 THEN 'EXPIRING_SOON'
            ELSE 'VALID'
        END AS ExpiryState
    FROM InventoryDB.dbo.product_batches b
    LEFT JOIN ProductDB.dbo.products p ON b.product_id = p.id
)
SELECT
    BatchId,
    ProductId,
    ProductName,
    ExpiryDate,
    RemainingDays,
    Quantity,
    IsPerishable
FROM BatchComputed
WHERE ExpiryState = 'EXPIRING_SOON'
ORDER BY RemainingDays ASC";

        return await _context.Database
            .SqlQueryRaw<ExpiringSoonBatchDto>(sql)
            .ToListAsync();
    }

    public async Task<IEnumerable<BatchDetailDto>> GetBatchesByWarehouseIdAsync(Guid warehouseId)
    {
        const string sql = @"
SELECT
    b.id AS BatchId,
    b.product_id AS ProductId,
    p.name AS ProductName,
    p.unit AS Unit,
    CAST(b.manufacturing_date AS DATETIME2) AS ManufacturingDate,
    CAST(b.expiry_date AS DATETIME2) AS ExpiryDate,
    p.shelf_life_days AS ShelfLifeDays,
    p.is_perishable AS IsPerishable,
    CAST(b.quantity AS decimal(18,2)) AS Quantity,
    b.status AS Status,
    DATEDIFF(day, GETUTCDATE(), b.expiry_date) AS RemainingDays,
    CASE
        WHEN b.expiry_date < GETUTCDATE() THEN 'EXPIRED'
        WHEN p.is_perishable = 0 AND DATEDIFF(day, GETUTCDATE(), b.expiry_date) <= 30 THEN 'EXPIRING_SOON'
        WHEN p.is_perishable = 1 AND p.shelf_life_days <= 3 AND DATEDIFF(day, GETUTCDATE(), b.expiry_date) <= 1 THEN 'EXPIRING_SOON'
        WHEN p.is_perishable = 1 AND p.shelf_life_days <= 7 AND DATEDIFF(day, GETUTCDATE(), b.expiry_date) <= 2 THEN 'EXPIRING_SOON'
        WHEN p.is_perishable = 1 AND p.shelf_life_days > 7 AND DATEDIFF(day, GETUTCDATE(), b.expiry_date) <= 3 THEN 'EXPIRING_SOON'
        ELSE 'VALID'
    END AS ExpiryState
FROM InventoryDB.dbo.product_batches b
LEFT JOIN ProductDB.dbo.products p ON b.product_id = p.id
WHERE b.warehouse_id = @WarehouseId";

        var warehouseIdParameter = new SqlParameter("@WarehouseId", warehouseId);

        return await _context.Database
            .SqlQueryRaw<BatchDetailDto>(sql, warehouseIdParameter)
            .ToListAsync();
    }
}
