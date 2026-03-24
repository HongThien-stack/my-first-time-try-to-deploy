using PosService.Application.DTOs;

namespace PosService.Application.Interfaces;

public interface IMomoPaymentService
{
    Task<MomoCreatePaymentResponse> CreatePaymentAsync(string saleNumber, decimal amount, Guid saleId, string paymentType = "WALLET");
    bool VerifySignature(MomoNotifyRequest request);
    string ComputeHmacSha256(string message);
}
