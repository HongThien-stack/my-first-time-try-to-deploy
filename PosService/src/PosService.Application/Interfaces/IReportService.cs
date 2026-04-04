using PosService.Application.DTOs;

namespace PosService.Application.Interfaces;

public interface IReportService
{
    Task<IReadOnlyList<RevenueTrendPointDto>> GetRevenueTrendAsync(RevenueTrendRequestDto request, Guid? storeId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TopProductReportDto>> GetTopProductsAsync(TopProductsRequestDto request, Guid? storeId, CancellationToken cancellationToken = default);
    Task<AdminTopProductsTrendResponseDto> GetAdminTopProductsTrendAsync(TopProductsRequestDto request, CancellationToken cancellationToken = default);
    Task<InventorySummaryDto> GetInventorySummaryAsync(Guid? storeId, int lowStockThreshold = 10, CancellationToken cancellationToken = default);
}
