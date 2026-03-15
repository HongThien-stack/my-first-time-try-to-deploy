using System.ComponentModel.DataAnnotations;

namespace InventoryService.Application.DTOs;

public class ProductBatchDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public Guid WarehouseId { get; set; }
    public string BatchNumber { get; set; } = string.Empty;
    public int Quantity { get; set; } // Total items in batch
    public DateTime? ManufacturingDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public string? Supplier { get; set; }
    public DateTime ReceivedAt { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class ReceiveFromSupplierDto
{
    [Required]
    public Guid RestockRequestId { get; set; }

    [Required]
    public Guid WarehouseId { get; set; }

    public Guid ReceivedBy { get; set; }

    public string? Notes { get; set; }

    [Required]
    public List<ReceiveItemFromSupplierDto> Items { get; set; } = new();
}

public class ReceiveItemFromSupplierDto
{
    [Required]
    public Guid ProductId { get; set; }

    [Required]
    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }

    [Required]
    public string BatchNumber { get; set; }

    public decimal? UnitPrice { get; set; }
    public string? SupplierName { get; set; }
    public Guid? SupplierId { get; set; }
    public DateTime? ManufacturingDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
}