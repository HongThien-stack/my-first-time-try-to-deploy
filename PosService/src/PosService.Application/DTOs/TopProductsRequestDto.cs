namespace PosService.Application.DTOs;

public class TopProductsRequestDto
{
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int TopN { get; set; } = 10;
}
