namespace PosService.Application.DTOs;

public class RevenueReportRequestDto
{
    // Only used by admin endpoint; manager endpoint always enforces claim-based storeId.
    public Guid? StoreId { get; set; }

    // DAY | MONTH | YEAR | RANGE
    public string FilterType { get; set; } = "DAY";

    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}
