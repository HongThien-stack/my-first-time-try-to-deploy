using InventoryService.Application.DTOs;
using InventoryService.Application.Interfaces;
using InventoryService.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace InventoryService.Application.Services;

public class StockMovementService : IStockMovementService
{
    private readonly IStockMovementRepository _movementRepository;
    private readonly IInventoryRepository _inventoryRepository;
    private readonly IProductBatchRepository _batchRepository;
    private readonly ILogger<StockMovementService> _logger;

    // Định nghĩa hằng số tránh hard code
    private const string MovementTypeInbound = "INBOUND";
    private const string LocationTypeWarehouse = "WAREHOUSE";
    private const string LocationTypeStore = "STORE";
    private const string StatusCompleted = "COMPLETED";
    private const string BatchStatusAvailable = "AVAILABLE";
    private const string MovementNumberPrefix = "SM";
    public StockMovementService(
        IStockMovementRepository movementRepository,
        IInventoryRepository inventoryRepository,
        IProductBatchRepository batchRepository,
        ILogger<StockMovementService> logger)

    {
        _movementRepository = movementRepository;
        _inventoryRepository = inventoryRepository;
        _batchRepository = batchRepository;
        _logger = logger;
    }

    #region GET METHODS
    public async Task<IEnumerable<StockMovementDto>> GetAllAsync()
    {
        _logger.LogInformation("Retrieving all stock movements");
        var movements = await _movementRepository.GetAllAsync();
        return movements.Select(MapToDto);
    }

    public async Task<StockMovementDto?> GetByIdAsync(Guid id)
    {
        _logger.LogInformation("Retrieving stock movement {MovementId}", id);
        var movement = await _movementRepository.GetByIdAsync(id);
        return movement != null ? MapToDto(movement) : null;
    }

    public async Task<IEnumerable<StockMovementItemDto>> GetItemsByMovementIdAsync(Guid movementId)
    {
        _logger.LogInformation("Retrieving items for stock movement {MovementId}", movementId);
        var movement = await _movementRepository.GetByIdAsync(movementId);
        if (movement == null)
        {
            throw new KeyNotFoundException($"Stock movement {movementId} not found");
        }
        return movement.StockMovementItems.Select(MapItemToDto);
    }

    #endregion

    #region POST /api/stock-movements/receive

    /// <summary>
    /// Nhập hàng từ Supplier vào Warehouse
    /// </summary>
    public async Task<StockMovementDto> ReceiveStockAsync(ReceiveStockRequestDto request, Guid receivedBy)
    {
        _logger.LogInformation("Receiving stock into warehouse {WarehouseId} from supplier {Supplier}",
            request.WarehouseId, request.Supplier);

        ValidateReceiveStockRequest(request);

        // sinh mã phiếu nhập
        var movementNumber = await GenerateMovementNumberAsync();

        //1. Tạo Stock movement header
        var movement = new StockMovement
        {
            Id = Guid.NewGuid(),
            MovementNumber = movementNumber,
            MovementType = MovementTypeInbound,
            LocationId = request.WarehouseId,
            LocationType = LocationTypeWarehouse,
            MovementDate = DateTime.UtcNow,
            Supplier = request.Supplier,
            PurchaseOrderId = request.PurchaseOrderId,
            TransferId = request.TransferId,
            ReceivedBy = receivedBy,
            Status = StatusCompleted,
            Notes = request.Notes
        };

        //2. Xử lý từng item: tạo batch + movement item + cập nhật inventory
        foreach (var item in request.Items)
        {
            var batch = await CreateBatchAsync(
                item.ProductId,
                request.WarehouseId,
                item.SlotId,
                item.BatchNumber,
                item.Quantity,
                item.ManufacturingDate,
                item.ExpiryDate,
                request.Supplier);

            movement.StockMovementItems.Add(new StockMovementItem
            {
                Id = Guid.NewGuid(),
                ProductId = item.ProductId,
                BatchId = batch.Id,
                SlotId = item.SlotId,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
            });

            await UpdateInventoryQuantityAsync(
                LocationTypeWarehouse, request.WarehouseId, item.ProductId, item.Quantity);
        }

        var created = await _movementRepository.AddAsync(movement);

        _logger.LogInformation("Stock received successfully. Movement: {MovementNumber}", movementNumber);
        return MapToDto(created);
    }
    #endregion

    #region POST /api/stock-movements/receive-perishable
    /// <summary>
    /// Nhập hàng từ Supplier vào Warehouse
    /// </summary>
    /// 
    public async Task<StockMovementDto> ReceivePerishableAsync(ReceivePerishableRequestDto request, Guid receivedBy)
    {
        _logger.LogInformation("Receiving perishable stock into store {StoreId} from supplier {Supplier}",
            request.StoreId, request.Supplier);

        ValidateReceivePerishableRequest(request);

        //Sinh mã phiếu nhập
        var movementNumber = await GenerateMovementNumberAsync();

        // 1. Tạo StockMovement header — LocationType là STORE thay vì WAREHOUSE
        var movement = new StockMovement
        {
            Id = Guid.NewGuid(),
            MovementNumber = movementNumber,
            MovementType = MovementTypeInbound,
            LocationId = request.StoreId,
            LocationType = LocationTypeStore,
            MovementDate = DateTime.UtcNow,
            Supplier = request.Supplier,
            ReceivedBy = receivedBy,
            Status = StatusCompleted,
            Notes = request.Notes
        };

        //2. Xử lý  từng item
        foreach (var item in request.Items)
        {
            // Hàng Tươi sống nhập thẳng vào store -> không có slotid
            // vẫn tạo batch để track hạn sử dụng (ExpiryDate bắt buộc)
            var batch = await CreateBatchAsync(
                item.ProductId,
                request.StoreId,
                slotId: null,
                item.BatchNumber,
                item.Quantity,
                item.ManufacturingDate,
                item.ExpiryDate,
                request.Supplier);

            movement.StockMovementItems.Add(new StockMovementItem
            {
                Id = Guid.NewGuid(),
                ProductId = item.ProductId,
                BatchId = batch.Id,
                SlotId = null,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
            });

            //Cộng số lượng vào bảng inventories (tại store)
            await UpdateInventoryQuantityAsync(
                LocationTypeStore, request.StoreId, item.ProductId, item.Quantity);
        }

        var created = await _movementRepository.AddAsync(movement);

        _logger.LogInformation("Perishable stock received successfully. Movement: {MovementNumber}", movementNumber);

        return MapToDto(created);
    }
    #endregion

    #region Private helpers
    /// <summary>
    /// Sinh mã phiếu nhập tự động theo format: SM-yyyyMMdd-001
    /// Đếm số movement trong ngày để tăng số thứ tự
    /// </summary>
    private async Task<string> GenerateMovementNumberAsync()
    {
        var today = DateTime.UtcNow;
        var count = await _movementRepository.CountByDateAsync(today);
        //VD: SM-123123123-001
        return $"{MovementNumberPrefix}-{today:yyyyMMdd}-{(count + 1):D3}";
    }
    /// <summary>
    /// Tạo ProductBatch mới.
    /// Nếu không truyền BatchNumber → tự sinh theo format: BATCH-yyyyMMdd-{guid 8 ký tự}
    /// </summary>
    private async Task<ProductBatch> CreateBatchAsync(
        Guid productId,
        Guid warehouseOrStoreId,
        Guid? slotId,
        string? batchNumber,
        int quantity,
        DateTime? manufacturingDate,
        DateTime? ExpiryDate,
        string supplier)
    {
        var batch = new ProductBatch
        {
            Id = Guid.NewGuid(),
            ProductId = productId,
            WarehouseId = warehouseOrStoreId,
            SlotId = slotId,
            BatchNumber = string.IsNullOrWhiteSpace(batchNumber) ? GenerateBatchNumber() : batchNumber,
            Quantity = quantity,
            ManufacturingDate = manufacturingDate,
            ExpiryDate = ExpiryDate,
            Supplier = supplier,
            ReceivedAt = DateTime.UtcNow,
            Status = BatchStatusAvailable
        };
        return await _batchRepository.AddAsync(batch);
    }

    /// <summary>
    /// Tự sinh batch number nếu client không truyền
    /// </summary>
    private static string GenerateBatchNumber()
    {
        var datePart = DateTime.UtcNow.ToString("yyyyMMdd");
        var uniquePart = Guid.NewGuid().ToString("N")[..8].ToUpperInvariant();
        return $"BATCH-{datePart}-{uniquePart}";
    }

    /// <summary>
    /// Cập nhật bảng inventories:
    /// - Nếu đã có record (cùng product + location) → cộng thêm quantity
    /// - Nếu chưa có → tạo mới
    /// </summary>
    private async Task UpdateInventoryQuantityAsync(
        string locationType, Guid locationId, Guid productId, int quantityToAdd)
    {
        var inventory = await _inventoryRepository.GetByLocationAndProductAsync(
            locationType, locationId, productId);
        if (inventory != null)
        {
            // đã tồn tại -> cộng thêm số lượng nhập
            inventory.Quantity += quantityToAdd;
            await _inventoryRepository.UpdateAsync(inventory);
        }
        else
        {
            // chưa có record inventory tại vị trí này -> tạo mới
            var newInventory = new Inventory
            {
                Id = Guid.NewGuid(),
                ProductId = productId,
                Quantity = quantityToAdd,
                LocationId = locationId,
                LocationType = locationType,
                ReservedQuantity = 0,
            };
            await _inventoryRepository.AddAsync(newInventory);
        }
    }

    /// <summary>
    /// Validate request nhập hàng vào warehouse
    /// </summary>

    private static void ValidateReceiveStockRequest(ReceiveStockRequestDto request)
    {
        if (request.WarehouseId == Guid.Empty)
            throw new ArgumentException("WarehouseId is required.");

        if (string.IsNullOrWhiteSpace(request.Supplier))
            throw new ArgumentException("Supplier is required.");

        if (request.Items == null || request.Items.Count == 0)
            throw new ArgumentException("At least one item is required.");
        foreach(var item in request.Items)
        {
            if (item.ProductId == Guid.Empty)
                throw new ArgumentException("ProductId is required for all items.");
            if (item.Quantity <= 0)
                throw new ArgumentException("Quantity must be greater than 0.");
        }    
    }

    /// <summary>
    /// Validate request nhập hàng tươi sống vào store.
    /// ExpiryDate bắt buộc vì hàng tươi sống phải track hạn dùng.
    /// </summary>
    private static void ValidateReceivePerishableRequest(ReceivePerishableRequestDto request)
    {
        if (request.StoreId == Guid.Empty)
            throw new ArgumentException("StoreId is required.");

        if (string.IsNullOrWhiteSpace(request.Supplier))
            throw new ArgumentException("Supplier is required");

        if (request.Items == null || request.Items.Count == 0)
            throw new ArgumentException("At least one item is required");

        foreach (var item in request.Items)
        {
            if (item.ProductId == Guid.Empty)
                throw new ArgumentException("ProductId is required for all item.");

            if (item.Quantity <= 0)
                throw new ArgumentException("Quantity must be greater than 0");

            // Hàng tươi sống BẮT BUỘC có hạn sử dụng
            if (item.ExpiryDate == default)
                throw new ArgumentException("ExpiryDate is required for perishable items.");

            if (item.ExpiryDate <= DateTime.UtcNow)
                throw new ArgumentException("ExpiryDate must be in the future.");
        }
    }
    #endregion

    #region Mapping

    private static StockMovementDto MapToDto(StockMovement m)
    {
        return new StockMovementDto
        {
            Id = m.Id,
            MovementNumber = m.MovementNumber,
            MovementType = m.MovementType,
            LocationId = m.LocationId,
            LocationType = m.LocationType,
            MovementDate = m.MovementDate,
            Supplier = m.Supplier,
            PurchaseOrderId = m.PurchaseOrderId,
            TransferId = m.TransferId,
            ReceivedBy = m.ReceivedBy,
            Status = m.Status,
            Notes = m.Notes,
            CreatedAt = m.CreatedAt,
            TotalItems = m.StockMovementItems.Count,
            Items = m.StockMovementItems.Select(MapItemToDto)
        };
    }

    private static StockMovementItemDto MapItemToDto(StockMovementItem i)
    {
        return new StockMovementItemDto
        {
            Id = i.Id,
            MovementId = i.MovementId,
            ProductId = i.ProductId,
            BatchId = i.BatchId,
            SlotId = i.SlotId,
            Quantity = i.Quantity,
            UnitPrice = i.UnitPrice
        };
    }
    #endregion
}
