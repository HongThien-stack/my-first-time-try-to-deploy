using InventoryService.Application.DTOs;
using InventoryService.Application.Interfaces;
using InventoryService.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace InventoryService.Application.Services;

public class InventoryManagementService : IInventoryService
{
    private readonly IInventoryRepository _inventoryRepository;
    private readonly IProductBatchRepository _productBatchRepository;
    private readonly IProductServiceClient _productServiceClient;
    private readonly ILogger<InventoryManagementService> _logger;

    public InventoryManagementService(
        IInventoryRepository inventoryRepository,
        IProductBatchRepository productBatchRepository,
        IProductServiceClient productServiceClient,
        ILogger<InventoryManagementService> logger)
    {
        _inventoryRepository = inventoryRepository;
        _productBatchRepository = productBatchRepository;
        _productServiceClient = productServiceClient;
        _logger = logger;
    }

    public async Task<IEnumerable<InventoryDto>> GetAllInventoriesAsync()
    {
        _logger.LogInformation("Getting all inventories");
        
        var inventories = await _inventoryRepository.GetAllAsync();
        return await MapToDtosWithProductDetailsAsync(inventories);
    }

    public async Task<InventoryDto?> GetInventoryByIdAsync(Guid id)
    {
        _logger.LogInformation("Getting inventory by ID: {InventoryId}", id);
        
        var inventory = await _inventoryRepository.GetByIdAsync(id);
        
        if (inventory == null) return null;
        
        var dtos = await MapToDtosWithProductDetailsAsync(new[] { inventory });
        return dtos.FirstOrDefault();
    }

    public async Task<IEnumerable<InventoryDto>> GetInventoriesByLocationAsync(string locationType, Guid locationId)
    {
        _logger.LogInformation("Getting inventories by location: {LocationType}:{LocationId}", locationType, locationId);
        
        var inventories = await _inventoryRepository.GetByLocationAsync(locationType, locationId);
        return await MapToDtosWithProductDetailsAsync(inventories);
    }

    public async Task<IEnumerable<InventoryDto>> GetInventoriesByProductAsync(Guid productId)
    {
        _logger.LogInformation("Getting inventories by product: {ProductId}", productId);
        
        var inventories = await _inventoryRepository.GetByProductIdAsync(productId);
        return await MapToDtosWithProductDetailsAsync(inventories);
    }

    public async Task<IEnumerable<InventoryDto>> GetLowStockItemsAsync(string? locationType = null)
    {
        _logger.LogInformation("Getting low stock items");
        
        var inventories = await _inventoryRepository.GetLowStockItemsAsync(locationType);
        return await MapToDtosWithProductDetailsAsync(inventories);
    }

    public async Task<LowStockAlertResponse> GetLowStockAlertsAsync(
        string? locationType = null,
        Guid? locationId = null,
        int pageNumber = 1,
        int pageSize = 20)
    {
        _logger.LogInformation(
            "Getting low stock alerts with filters - LocationType: {LocationType}, LocationId: {LocationId}, Page: {PageNumber}, PageSize: {PageSize}",
            locationType, locationId, pageNumber, pageSize);

        // Get paginated low stock inventory items
        var (inventories, totalCount) = await _inventoryRepository.GetLowStockAlertsAsync(
            locationType, locationId, pageNumber, pageSize);

        var inventoryList = inventories.ToList();

        // Fetch product details in batch
        var productIds = inventoryList.Select(i => i.ProductId).Distinct().ToList();
        var productInfoMap = await _productServiceClient.GetProductsByIdsAsync(productIds);

        // Map to DTOs with enriched product information
        var alertItems = inventoryList.Select(inv =>
        {
            var productInfo = productInfoMap.GetValueOrDefault(inv.ProductId);
            // Calculate available quantity (since AvailableQuantity is a computed column not mapped by EF)
            var availableQty = inv.Quantity - inv.ReservedQuantity;

            return new LowStockAlertDto
            {
                ProductId = inv.ProductId,
                ProductName = productInfo?.Name ?? "Unknown Product",
                Sku = $"SKU-{inv.ProductId.ToString().Substring(0, 8).ToUpper()}", // Generate SKU if not available
                Unit = productInfo?.Unit ?? "unit",
                LocationType = inv.LocationType,
                LocationId = inv.LocationId,
                AvailableQuantity = availableQty,
                MinStockLevel = inv.MinStockLevel ?? 0,
                MaxStockLevel = inv.MaxStockLevel ?? 0,
                StockStatus = availableQty == 0 ? "OUT_OF_STOCK" : "LOW",
                LastStockCheck = inv.LastStockCheck,
                UpdatedAt = inv.UpdatedAt
            };
        }).ToList();

        // Calculate total pages
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        return new LowStockAlertResponse
        {
            Items = alertItems,
            TotalItems = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalPages = totalPages
        };
    }

    public async Task<InventoryDto> UpdateMinStockLevelAsync(Guid inventoryId, int minStockLevel)
    {
        _logger.LogInformation(
            "Updating min stock level for inventory {InventoryId} to {MinStockLevel}",
            inventoryId,
            minStockLevel);

        if (minStockLevel < 0)
        {
            throw new ArgumentException("Min stock level must be greater than or equal to 0");
        }

        var inventory = await _inventoryRepository.GetByIdAsync(inventoryId);
        if (inventory == null)
        {
            throw new KeyNotFoundException($"Inventory {inventoryId} not found");
        }

        if (inventory.MaxStockLevel.HasValue && minStockLevel > inventory.MaxStockLevel.Value)
        {
            throw new ArgumentException("Min stock level cannot be greater than max stock level");
        }

        inventory.MinStockLevel = minStockLevel;
        inventory.UpdatedAt = DateTime.UtcNow;

        await _inventoryRepository.UpdateAsync(inventory);

        return MapToDto(inventory);
    }

    public async Task<InventoryDto> UpdateInventoryAsync(Guid id, int quantity, Guid performedBy, string reason)
    {
        _logger.LogInformation("Updating inventory {InventoryId} to quantity {Quantity}", id, quantity);
        
        var inventory = await _inventoryRepository.GetByIdAsync(id);
        if (inventory == null)
        {
            throw new KeyNotFoundException($"Inventory {id} not found");
        }

        inventory.Quantity = quantity;
        inventory.UpdatedAt = DateTime.UtcNow;

        await _inventoryRepository.UpdateAsync(inventory);
        
        return MapToDto(inventory);
    }

    public async Task UpdateReservedQuantityAsync(Inventory inventory)
    {
        await _inventoryRepository.UpdateReservedQuantityAsync(inventory);
    }

    public async Task<InventoryDto> CheckOrCreateInventoryAsync(CreateInventoryDto dto)
    {
        _logger.LogInformation("Checking or creating inventory for product {ProductId} at location {LocationType}:{LocationId}", 
            dto.ProductId, dto.LocationType, dto.LocationId);

        // Check if inventory already exists
        var existingInventory = await _inventoryRepository.GetByLocationAndProductAsync(dto.LocationType, dto.LocationId, dto.ProductId);
        
        if (existingInventory != null)
        {
            _logger.LogInformation("Inventory already exists for product {ProductId} at location {LocationId}", 
                dto.ProductId, dto.LocationId);
            return MapToDto(existingInventory);
        }

        // Create new inventory if it doesn't exist
        var newInventory = new Inventory
        {
            Id = Guid.NewGuid(),
            ProductId = dto.ProductId,
            LocationType = dto.LocationType,
            LocationId = dto.LocationId,
            Quantity = dto.Quantity,
            ReservedQuantity = 0,
            MinStockLevel = dto.MinStockLevel ?? 10,
            MaxStockLevel = dto.MaxStockLevel ?? 1000,
            LastStockCheck = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var createdInventory = await _inventoryRepository.AddAsync(newInventory);
        _logger.LogInformation("New inventory created with ID {InventoryId}", createdInventory.Id);

        return MapToDto(createdInventory);
    }

    /// <summary>
    /// Trừ tồn kho khi sale complete (CASH) hoặc Momo payment success
    /// Áp dụng FEFO (First Expiration First Out): trừ lô hàng với ngày hết hạn sớm nhất trước
    /// </summary>
    public async Task<ReduceInventoryResponseDto> ReduceInventoryAsync(ReduceInventoryRequestDto request, Guid performedBy)
    {
        _logger.LogInformation("Reducing inventory for store {StoreId} with {ItemCount} items (FEFO logic)", 
            request.StoreId, request.Items.Count);

        var response = new ReduceInventoryResponseDto { Success = true };
        var reducedItems = new List<ReducedInventoryItemDto>();

        try
        {
            foreach (var item in request.Items)
            {
                // Tìm inventory: ProductId + StoreId (location_id)
                var inventory = await _inventoryRepository.GetByLocationAndProductAsync(
                    "STORE", request.StoreId, item.ProductId);

                if (inventory == null)
                {
                    _logger.LogWarning("Inventory not found for product {ProductId} at store {StoreId}", 
                        item.ProductId, request.StoreId);
                    throw new KeyNotFoundException(
                        $"Inventory not found for product {item.ProductId} at store {request.StoreId}");
                }

                // Kiểm tra đủ tồn kho
                if (inventory.Quantity < item.Quantity)
                {
                    _logger.LogError("Insufficient inventory for product {ProductId}. Required: {Required}, Available: {Available}", 
                        item.ProductId, item.Quantity, inventory.AvailableQuantity);
                    throw new InvalidOperationException(
                        $"Insufficient inventory for product {item.ProductId}. Required: {item.Quantity}, Available: {inventory.AvailableQuantity}");
                }

                // Bước 1: Trừ tồn kho chính
                inventory.Quantity -= item.Quantity;
                inventory.UpdatedAt = DateTime.UtcNow;
                await _inventoryRepository.UpdateAsync(inventory);

                _logger.LogInformation("Inventory reduced for product {ProductId}: -{Quantity}, Remaining: {Remaining}", 
                    item.ProductId, item.Quantity, inventory.Quantity);

                // Bước 2: Trừ lô hàng theo FEFO (First Expiration First Out)
                await ReduceBatchesByFEFOAsync(request.StoreId, item.ProductId, item.Quantity);

                reducedItems.Add(new ReducedInventoryItemDto
                {
                    ProductId = item.ProductId,
                    QuantityReduced = item.Quantity,
                    RemainingQuantity = inventory.Quantity
                });
            }

            response.ReducedItems = reducedItems;
            response.Message = $"Successfully reduced inventory and batches for {reducedItems.Count} items";

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reducing inventory for store {StoreId}", request.StoreId);
            response.Success = false;
            response.Message = $"Error reducing inventory: {ex.Message}";
            throw;
        }
    }

    /// <summary>
    /// Trừ lô hàng với logic FEFO: ngày hết hạn sớm nhất được ưu tiên trừ trước
    /// Nếu lô hàng không đủ, chuyển sang lô hàng tiếp theo với ngày hết hạn gần hơn
    /// </summary>
    private async Task ReduceBatchesByFEFOAsync(Guid storeId, Guid productId, int quantityToReduce)
    {
        try
        {
            // Lấy tất cả lô hàng của sản phẩm tại cửa hàng (store)
            // Stores được lưu với WarehouseId = StoreId trong hệ thống
            var allBatches = await _productBatchRepository.GetByWarehouseIdAsync(storeId);
            
            // Lọc batches của sản phẩm này và có status AVAILABLE, sắp xếp theo ngày hết hạn sớm nhất
            var batchesForProduct = allBatches
                .Where(b => b.ProductId == productId && b.Status == "AVAILABLE")
                .OrderBy(b => b.ExpiryDate ?? DateTime.MaxValue) // NULL expiry date = last (never expires)
                .ThenBy(b => b.ReceivedAt) // Nếu cùng ngày hết hạn, lấy batch cũ hơn trước (FIFO)
                .ToList();

            if (!batchesForProduct.Any())
            {
                _logger.LogWarning("No available batches found for product {ProductId} at store {StoreId}", 
                    productId, storeId);
                return;
            }

            int remainingQty = quantityToReduce;
            _logger.LogInformation("Starting FEFO batch reduction for product {ProductId}: need to reduce {Quantity} units from {BatchCount} batches",
                productId, quantityToReduce, batchesForProduct.Count);

            foreach (var batch in batchesForProduct)
            {
                if (remainingQty <= 0) break;

                int qtyToReduceFromBatch = Math.Min(remainingQty, batch.Quantity);
                batch.Quantity -= qtyToReduceFromBatch;
                remainingQty -= qtyToReduceFromBatch;

                // Đánh dấu batch là SOLD nếu quantity về 0
                if (batch.Quantity <= 0)
                {
                    batch.Status = "SOLD";
                    _logger.LogInformation("Batch {BatchId} ({BatchNumber}) marked as SOLD after reduction for product {ProductId}",
                        batch.Id, batch.BatchNumber, productId);
                }
                else
                {
                    _logger.LogInformation("Batch {BatchId} ({BatchNumber}) reduced by {ReducedQty}, remaining: {Remaining}",
                        batch.Id, batch.BatchNumber, qtyToReduceFromBatch, batch.Quantity);
                }

                await _productBatchRepository.UpdateAsync(batch);
            }

            if (remainingQty > 0)
            {
                _logger.LogWarning("Could not fully reduce batch quantity for product {ProductId}. Remaining: {Remaining}. This should not happen as inventory check passed.",
                    productId, remainingQty);
            }
            else
            {
                _logger.LogInformation("Successfully completed FEFO batch reduction for product {ProductId}: {Quantity} units reduced",
                    productId, quantityToReduce);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reducing batches for product {ProductId} at store {StoreId}", productId, storeId);
            // Don't throw - batch reduction is auxiliary to inventory reduction
            // Inventory is already reduced, batches are just for tracking
        }
    }

    /// <summary>
    /// Map Inventory entities to DTOs with product details (unit) fetched from ProductService
    /// </summary>
    private async Task<IEnumerable<InventoryDto>> MapToDtosWithProductDetailsAsync(IEnumerable<Inventory> inventories)
    {
        var inventoryList = inventories.ToList();
        if (!inventoryList.Any())
            return Enumerable.Empty<InventoryDto>();

        // Batch fetch product details for all inventory items
        var productIds = inventoryList.Select(i => i.ProductId).Distinct().ToList();
        var productDetailsMap = await _productServiceClient.GetProductsByIdsAsync(productIds);

        // Map to DTOs with product unit included
        return inventoryList.Select(inv =>
        {
            var productDetail = productDetailsMap.GetValueOrDefault(inv.ProductId);
            return new InventoryDto
            {
                Id = inv.Id,
                ProductId = inv.ProductId,
                LocationType = inv.LocationType,
                LocationId = inv.LocationId,
                Quantity = inv.Quantity,
                ReservedQuantity = inv.ReservedQuantity,
                AvailableQuantity = inv.AvailableQuantity,
                MinStockLevel = inv.MinStockLevel,
                MaxStockLevel = inv.MaxStockLevel,
                LastStockCheck = inv.LastStockCheck,
                UpdatedAt = inv.UpdatedAt,
                Unit = productDetail?.Unit ?? "unit" // Default to "unit" if not found
            };
        }).ToList();
    }

    private InventoryDto MapToDto(Inventory inventory)
    {
        return new InventoryDto
        {
            Id = inventory.Id,
            ProductId = inventory.ProductId,
            LocationType = inventory.LocationType,
            LocationId = inventory.LocationId,
            Quantity = inventory.Quantity,
            ReservedQuantity = inventory.ReservedQuantity,
            AvailableQuantity = inventory.AvailableQuantity,
            MinStockLevel = inventory.MinStockLevel,
            MaxStockLevel = inventory.MaxStockLevel,
            LastStockCheck = inventory.LastStockCheck,
            UpdatedAt = inventory.UpdatedAt,
            Unit = null // Use MapToDtosWithProductDetailsAsync() to populate unit
        };
    }
}
