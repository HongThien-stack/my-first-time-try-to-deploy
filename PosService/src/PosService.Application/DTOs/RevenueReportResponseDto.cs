namespace PosService.Application.DTOs;

public class RevenueReportResponseDto
{
    public decimal TotalRevenue { get; set; }
    public int TotalOrders { get; set; }
    public int TotalProductsSold { get; set; }
    public List<TopRevenueProductDto> TopProducts { get; set; } = new();
}

public class TopRevenueProductDto
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int QuantitySold { get; set; }
    public decimal Revenue { get; set; }
}
