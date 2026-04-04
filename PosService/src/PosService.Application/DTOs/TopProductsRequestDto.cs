namespace PosService.Application.DTOs;

public class TopProductsRequestDto
{
    // Period: TODAY | YESTERDAY | THIS_MONTH | LAST_7_DAYS | CUSTOM (preferred over dates)
    public string Period { get; set; } = "";

    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int TopN { get; set; } = 10;
}
