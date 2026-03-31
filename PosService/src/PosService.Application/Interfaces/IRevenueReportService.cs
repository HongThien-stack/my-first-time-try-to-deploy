using PosService.Application.DTOs;

namespace PosService.Application.Interfaces;

public interface IRevenueReportService
{
    Task<RevenueReportResponseDto> GetManagerRevenueAsync(Guid managerStoreId, RevenueReportRequestDto request, CancellationToken cancellationToken = default);
    Task<RevenueReportResponseDto> GetAdminRevenueAsync(RevenueReportRequestDto request, CancellationToken cancellationToken = default);
}
