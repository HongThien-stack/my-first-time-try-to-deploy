using PosService.Application.DTOs;

namespace PosService.Application.Interfaces;

public interface IReceiptRepository
{
    Task<ReceiptResponseDto?> GetReceiptBySaleIdAsync(Guid saleId, CancellationToken cancellationToken = default);
}
