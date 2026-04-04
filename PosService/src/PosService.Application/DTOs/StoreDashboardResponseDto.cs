namespace PosService.Application.DTOs;

public class StoreDashboardResponseDto
{
    public string Period { get; set; } = string.Empty;
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }

    public RevenueReportResponseDto Revenue { get; set; } = new();
    public IReadOnlyList<RevenueTrendPointDto> RevenueTrend { get; set; } = Array.Empty<RevenueTrendPointDto>();
    public IReadOnlyList<TopProductReportDto> TopProducts { get; set; } = Array.Empty<TopProductReportDto>();
    public InventorySummaryDto InventorySummary { get; set; } = new();
}