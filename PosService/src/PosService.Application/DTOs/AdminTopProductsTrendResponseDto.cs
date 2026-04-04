namespace PosService.Application.DTOs;

public class AdminTopProductsTrendResponseDto
{
    public Guid? SelectedStoreId { get; set; }
    public string Period { get; set; } = string.Empty;
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public int TopN { get; set; }
    public IReadOnlyList<TopProductReportDto> OverallTopProducts { get; set; } = Array.Empty<TopProductReportDto>();
    public IReadOnlyList<StoreTopProductsDto> StoreTopProducts { get; set; } = Array.Empty<StoreTopProductsDto>();
}

public class StoreTopProductsDto
{
    public Guid StoreId { get; set; }
    public int TotalOrders { get; set; }
    public decimal TotalRevenue { get; set; }
    public IReadOnlyList<TopProductReportDto> TopProducts { get; set; } = Array.Empty<TopProductReportDto>();
}
