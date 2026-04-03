namespace PosService.Application.DTOs;

public class RevenueTrendRequestDto
{
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }

    // DAY | MONTH | YEAR
    public string GroupBy { get; set; } = "DAY";
}
