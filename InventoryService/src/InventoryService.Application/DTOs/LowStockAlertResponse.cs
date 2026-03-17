namespace InventoryService.Application.DTOs;

/// <summary>
/// Paginated response for low stock alerts
/// </summary>
public class LowStockAlertResponse
{
    public List<LowStockAlertDto> Items { get; set; } = new();
    public int TotalItems { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public bool HasPrevious => PageNumber > 1;
    public bool HasNext => PageNumber < TotalPages;
}
