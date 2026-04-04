namespace PosService.Application.DTOs;

public class RevenueTrendRequestDto
{
    // Period: TODAY | YESTERDAY | THIS_MONTH | LAST_7_DAYS | CUSTOM (preferred over dates)
    public string Period { get; set; } = "";

    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }

    // DAY | MONTH | YEAR
    public string GroupBy { get; set; } = "DAY";
}
