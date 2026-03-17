using PosService.Application.DTOs;

namespace PosService.Application.Interfaces;

public interface IReceiptService
{
    Task<ReceiptResponseDto?> GetReceiptAsync(Guid saleId, CancellationToken cancellationToken = default);
}
