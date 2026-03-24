namespace InventoryService.Application.DTOs;

public class UpdateMinStockLevelDto
{
    /// <summary>
    /// Minimum stock level threshold. Must be >= 0.
    /// </summary>
    public int MinStockLevel { get; set; }
}
