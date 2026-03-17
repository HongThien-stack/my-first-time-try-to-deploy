using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using PosService.Application.DTOs;
using PosService.Application.Interfaces;

namespace PosService.Application.Services;

public class MomoPaymentService : IMomoPaymentService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _config;

    // Momo Configuration
    private string PartnerCode => _config["Momo:PartnerCode"] ?? "MOMOBKUN20180529";
    private string AccessKey => _config["Momo:AccessKey"] ?? "klm05TvNBzhg7h7j";
    private string SecretKey => _config["Momo:SecretKey"] ?? "at67qH6mk8w5Y1nAyMoYKMWACiEi2bsa";
    private string Endpoint => _config["Momo:Endpoint"] ?? "https://test-payment.momo.vn/v2/gateway/api/create";
    private string ReturnUrl => _config["Momo:ReturnUrl"] ?? "http://localhost:5000/api/payment/momo/return";
    private string NotifyUrl => _config["Momo:NotifyUrl"] ?? "http://localhost:5000/api/payment/momo/notify";

    public MomoPaymentService(HttpClient httpClient, IConfiguration config)
    {
        _httpClient = httpClient;
        _config = config;
    }

    /// <summary>
    /// Tạo thanh toán Momo và nhận QR Code hoặc Payment URL
    /// </summary>
    /// <param name="paymentType">WALLET (QR), ATM, CREDIT_CARD</param>
    public async Task<MomoCreatePaymentResponse> CreatePaymentAsync(
        string saleNumber,
        decimal amount,
        Guid saleId,
        string paymentType = "WALLET")
    {
        // Tạo orderId UNIQUE bằng cách thêm timestamp
        var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmssfff");
        var orderId = $"{saleNumber}-{timestamp}"; // VD: SALE-TEST-MOMO-20260312153045123
        
        var requestId = Guid.NewGuid().ToString();
        var orderInfo = $"Thanh toan don hang {saleNumber}";
        
        // Map payment type to Momo requestType
        var requestType = paymentType.ToUpper() switch
        {
            "ATM" => "payWithATM",
            "CREDIT_CARD" => "payWithCC",
            _ => "captureWallet" // Default: QR Code/Wallet
        };
        
        var extraData = Convert.ToBase64String(Encoding.UTF8.GetBytes(saleId.ToString()));

        // Tạo chữ ký HMAC SHA256 theo docs Momo
        var rawSignature = $"accessKey={AccessKey}" +
                          $"&amount={amount:F0}" +
                          $"&extraData={extraData}" +
                          $"&ipnUrl={NotifyUrl}" +
                          $"&orderId={orderId}" +
                          $"&orderInfo={orderInfo}" +
                          $"&partnerCode={PartnerCode}" +
                          $"&redirectUrl={ReturnUrl}" +
                          $"&requestId={requestId}" +
                          $"&requestType={requestType}";

        var signature = ComputeHmacSha256(rawSignature, SecretKey);

        var requestBody = new
        {
            partnerCode = PartnerCode,
            accessKey = AccessKey,
            requestId = requestId,
            amount = amount.ToString("F0"), // Không dấu phẩy, VD: "378000"
            orderId = orderId,
            orderInfo = orderInfo,
            redirectUrl = ReturnUrl,
            ipnUrl = NotifyUrl,
            requestType = requestType,
            extraData = extraData,
            lang = "vi",
            signature = signature
        };

        var json = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        try
        {
            var response = await _httpClient.PostAsync(Endpoint, content);
            var responseBody = await response.Content.ReadAsStringAsync();

            var result = JsonSerializer.Deserialize<MomoCreatePaymentResponse>(responseBody,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return result ?? new MomoCreatePaymentResponse
            {
                ResultCode = -1,
                Message = "Failed to deserialize Momo response"
            };
        }
        catch (Exception ex)
        {
            return new MomoCreatePaymentResponse
            {
                ResultCode = -1,
                Message = $"Error calling Momo API: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Xác thực chữ ký từ Momo IPN callback
    /// </summary>
    public bool VerifySignature(MomoNotifyRequest request)
    {
        var rawSignature = $"accessKey={AccessKey}" +
                          $"&amount={request.Amount}" +
                          $"&extraData={request.ExtraData}" +
                          $"&message={request.Message}" +
                          $"&orderId={request.OrderId}" +
                          $"&orderInfo={request.OrderInfo}" +
                          $"&orderType={request.OrderType}" +
                          $"&partnerCode={request.PartnerCode}" +
                          $"&payType={request.PayType}" +
                          $"&requestId={request.RequestId}" +
                          $"&responseTime={request.ResponseTime}" +
                          $"&resultCode={request.ResultCode}" +
                          $"&transId={request.TransId}";

        var expectedSignature = ComputeHmacSha256(rawSignature, SecretKey);
        return expectedSignature.Equals(request.Signature, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Tính toán HMAC SHA256 signature
    /// </summary>
    private string ComputeHmacSha256(string message, string key)
    {
        var keyBytes = Encoding.UTF8.GetBytes(key);
        var messageBytes = Encoding.UTF8.GetBytes(message);

        using var hmac = new HMACSHA256(keyBytes);
        var hashBytes = hmac.ComputeHash(messageBytes);
        return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
    }
}
