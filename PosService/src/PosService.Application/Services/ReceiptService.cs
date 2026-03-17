using Microsoft.Extensions.Logging;
using PosService.Application.DTOs;
using PosService.Application.Interfaces;

namespace PosService.Application.Services;

public class ReceiptService : IReceiptService
{
    private readonly IReceiptRepository _receiptRepository;
    private readonly ILogger<ReceiptService> _logger;

    public ReceiptService(IReceiptRepository receiptRepository, ILogger<ReceiptService> logger)
    {
        _receiptRepository = receiptRepository;
        _logger = logger;
    }

    public async Task<ReceiptResponseDto?> GetReceiptAsync(Guid saleId, CancellationToken cancellationToken = default)
    {
        if (saleId == Guid.Empty)
        {
            _logger.LogWarning("Receipt request rejected due to empty sale id");
            return null;
        }

        return await _receiptRepository.GetReceiptBySaleIdAsync(saleId, cancellationToken);
    }
}
