namespace InventoryService.Application.DTOs;

public class CreateInventoryDto
{
    /// <summary>
    /// Product ID (referenced from ProductDB)
    /// </summary>
    public Guid ProductId { get; set; }

    /// <summary>
    /// Location Type: WAREHOUSE or STORE
    /// </summary>
    public string LocationType { get; set; } = "WAREHOUSE";

    /// <summary>
    /// Location ID (warehouse_id or store_id)
    /// </summary>
    public Guid LocationId { get; set; }

    /// <summary>
    /// Initial quantity (optional, default 0)
    /// </summary>
    public int Quantity { get; set; } = 0;

    /// <summary>
    /// Minimum stock level threshold (optional, default 10)
    /// </summary>
    public int? MinStockLevel { get; set; } = 10;

    /// <summary>
    /// Maximum stock level (optional, default 1000)
    /// </summary>
    public int? MaxStockLevel { get; set; } = 1000;
}
