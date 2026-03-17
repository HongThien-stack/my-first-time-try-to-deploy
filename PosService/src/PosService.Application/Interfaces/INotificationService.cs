using PosService.Application.DTOs;

namespace PosService.Application.Interfaces;

public interface INotificationService
{
    Task SendReceiptAsync(
        string method,
        string destination,
        ReceiptResponseDto receipt,
        byte[]? pdfAttachment,
        CancellationToken cancellationToken = default);
}
