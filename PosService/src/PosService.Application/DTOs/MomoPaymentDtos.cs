using System.Text.Json.Serialization;

namespace PosService.Application.DTOs;

/// <summary>
/// Request để tạo thanh toán Momo
/// </summary>
public class CreateMomoPaymentRequest
{
    public Guid SaleId { get; set; }
    
    /// <summary>
    /// Loại thanh toán: WALLET (QR), ATM, CREDIT_CARD
    /// </summary>
    public string PaymentType { get; set; } = "WALLET";
}

/// <summary>
/// Response từ Momo khi tạo payment
/// </summary>
public class MomoCreatePaymentResponse
{
    [JsonPropertyName("partnerCode")]
    public string PartnerCode { get; set; } = string.Empty;

    [JsonPropertyName("requestId")]
    public string RequestId { get; set; } = string.Empty;

    [JsonPropertyName("orderId")]
    public string OrderId { get; set; } = string.Empty;

    [JsonPropertyName("amount")]
    public long Amount { get; set; }

    [JsonPropertyName("responseTime")]
    public long ResponseTime { get; set; }

    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    [JsonPropertyName("resultCode")]
    public int ResultCode { get; set; }

    [JsonPropertyName("payUrl")]
    public string PayUrl { get; set; } = string.Empty;

    [JsonPropertyName("deeplink")]
    public string Deeplink { get; set; } = string.Empty;

    [JsonPropertyName("qrCodeUrl")]
    public string QrCodeUrl { get; set; } = string.Empty;

    public bool IsSuccess => ResultCode == 0;
}

/// <summary>
/// Request từ Momo IPN callback
/// </summary>
public class MomoNotifyRequest
{
    [JsonPropertyName("partnerCode")]
    public string PartnerCode { get; set; } = string.Empty;

    [JsonPropertyName("orderId")]
    public string OrderId { get; set; } = string.Empty;

    [JsonPropertyName("requestId")]
    public string RequestId { get; set; } = string.Empty;

    [JsonPropertyName("amount")]
    public long Amount { get; set; }

    [JsonPropertyName("orderInfo")]
    public string OrderInfo { get; set; } = string.Empty;

    [JsonPropertyName("orderType")]
    public string OrderType { get; set; } = string.Empty;

    [JsonPropertyName("transId")]
    public long TransId { get; set; }

    [JsonPropertyName("resultCode")]
    public int ResultCode { get; set; }

    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    [JsonPropertyName("payType")]
    public string PayType { get; set; } = string.Empty;

    [JsonPropertyName("responseTime")]
    public long ResponseTime { get; set; }

    [JsonPropertyName("extraData")]
    public string ExtraData { get; set; } = string.Empty;

    [JsonPropertyName("signature")]
    public string Signature { get; set; } = string.Empty;

    public bool IsSuccess => ResultCode == 0;
}

/// <summary>
/// Response sau khi tạo payment thành công
/// </summary>
public class CreateMomoPaymentResponseDto
{
    public bool Success { get; set; }
    public Guid PaymentId { get; set; }
    public string QrCodeUrl { get; set; } = string.Empty;
    public string Deeplink { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string PayUrl { get; set; } = string.Empty;
}
