namespace PosService.Application.DTOs;

public class InventorySummaryDto
{
    public int TotalProducts { get; set; }
    public int TotalStock { get; set; }
    public int LowStock { get; set; }
    public int OutOfStock { get; set; }
}
