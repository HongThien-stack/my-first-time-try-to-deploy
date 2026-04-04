namespace PosService.Application.DTOs;

public class StoreDashboardRequestDto
{
    // TODAY | YESTERDAY | LAST_7_DAYS | THIS_MONTH | CUSTOM
    public string Period { get; set; } = "LAST_7_DAYS";

    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }

    // DAY | MONTH | YEAR
    public string GroupBy { get; set; } = "DAY";

    public int TopN { get; set; } = 5;

    public int LowStockThreshold { get; set; } = 10;
}