using PosService.Application.DTOs;

namespace PosService.Application.Interfaces;

public interface IPdfReceiptService
{
    Task<byte[]> GenerateReceiptPdfAsync(ReceiptResponseDto receipt, CancellationToken cancellationToken = default);
}
