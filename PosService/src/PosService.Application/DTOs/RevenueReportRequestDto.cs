namespace PosService.Application.DTOs;

public class RevenueReportRequestDto
{
    // Only used by admin endpoint; manager endpoint always enforces claim-based storeId.
    public Guid? StoreId { get; set; }

    // Period: TODAY | YESTERDAY | THIS_MONTH | LAST_7_DAYS | CUSTOM (preferred over FilterType)
    // If not specified, falls back to FilterType
    public string Period { get; set; } = "";

    // Legacy: DAY | MONTH | YEAR | RANGE (fallback if Period not specified)
    public string FilterType { get; set; } = "DAY";

    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}
