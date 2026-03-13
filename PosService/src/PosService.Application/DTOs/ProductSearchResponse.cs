namespace PosService.Application.DTOs;

/// <summary>
/// Paginated response for product search results
/// </summary>
public class ProductSearchResponse
{
    public List<ProductSearchDto> Items { get; set; } = new();
    public int TotalItems { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public bool HasPrevious => PageNumber > 1;
    public bool HasNext => PageNumber < TotalPages;
}
