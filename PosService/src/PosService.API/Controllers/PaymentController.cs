using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using PosService.Application.DTOs;
using PosService.Application.Interfaces;
using PosService.Application.Interfaces.Http;
using PosService.Domain.Entities;
using PosService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace PosService.API.Controllers;

/// <summary>
/// Payment Controller - Xử lý thanh toán Momo/VNPay
/// </summary>
[ApiController]
[Route("api/payment")]
[Produces("application/json")]
public class PaymentController : ControllerBase
{
    private readonly IMomoPaymentService _momoService;
    private readonly IInventoryServiceClient _inventoryServiceClient;
    private readonly PosDbContext _dbContext;
    private readonly IConfiguration _config;
    private readonly ILogger<PaymentController> _logger;

    public PaymentController(
        IMomoPaymentService momoService,
        IInventoryServiceClient inventoryServiceClient,
        PosDbContext dbContext,
        IConfiguration config,
        ILogger<PaymentController> logger)
    {
        _momoService = momoService;
        _inventoryServiceClient = inventoryServiceClient;
        _dbContext = dbContext;
        _config = config;
        _logger = logger;
    }

    /// <summary>
    /// Tạo thanh toán Momo (QR, ATM, hoặc Credit Card)
    /// </summary>
    /// <param name="saleId">ID của sale cần thanh toán</param>
    /// <param name="paymentType">Loại thanh toán: WALLET (QR), ATM, CREDIT_CARD</param>
    /// <returns>QR Code URL hoặc Payment URL để thanh toán</returns>
    /// <response code="200">Trả về QR Code URL (WALLET) hoặc Payment URL (ATM/CREDIT_CARD)</response>
    /// <response code="404">Không tìm thấy sale</response>
    /// <response code="400">Lỗi khi gọi Momo API</response>
    [HttpPost("momo/create/{saleId}")]
    [ProducesResponseType(typeof(CreateMomoPaymentResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateMomoPayment(
        Guid saleId, 
        [FromQuery] string paymentType = "WALLET")
    {
        // Tìm sale
        var sale = await _dbContext.Sales.FirstOrDefaultAsync(s => s.Id == saleId);
        if (sale == null)
        {
            return NotFound(new { message = "Sale not found" });
        }

        // Kiểm tra đã thanh toán chưa
        if (sale.PaymentStatus == "COMPLETED")
        {
            return BadRequest(new { message = "Sale already paid" });
        }

        // Gọi Momo API
        var momoResponse = await _momoService.CreatePaymentAsync(
            sale.SaleNumber,
            sale.TotalAmount,
            sale.Id,
            paymentType
        );

        if (momoResponse.IsSuccess)
        {
            // Lưu payment với status PENDING
            var payment = new Payment
            {
                Id = Guid.NewGuid(),
                SaleId = sale.Id,
                PaymentMethod = "MOMO",
                Amount = sale.TotalAmount,
                Status = "PENDING",
                TransactionReference = momoResponse.RequestId,
                CreatedAt = DateTime.UtcNow,
                PaymentDate = DateTime.UtcNow
            };

            await _dbContext.Payments.AddAsync(payment);
            await _dbContext.SaveChangesAsync();

            // Update sale status
            sale.PaymentStatus = "PENDING";
            sale.PaymentMethod = "MOMO";
            sale.UpdatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();

            return Ok(new CreateMomoPaymentResponseDto
            {
                Success = true,
                PaymentId = payment.Id,
                QrCodeUrl = momoResponse.PayUrl,
                PayUrl = momoResponse.PayUrl,
                Deeplink = momoResponse.Deeplink,
                Message = "Vui lòng quét QR code hoặc mở Momo App để thanh toán"
            });
        }

        return BadRequest(new
        {
            success = false,
            message = momoResponse.Message,
            resultCode = momoResponse.ResultCode
        });
    }

    /// <summary>
    /// Webhook nhận thông báo từ Momo (IPN)
    /// </summary>
    /// <param name="request">Thông tin callback từ Momo</param>
    /// <returns>Xác nhận đã nhận callback</returns>
    /// <response code="200">Đã xử lý callback thành công</response>
    /// <response code="400">Chữ ký không hợp lệ</response>
    [HttpPost("momo/notify")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> MomoNotify([FromBody] MomoNotifyRequest request)
    {
        // Verify signature để đảm bảo request từ Momo
        if (!_momoService.VerifySignature(request))
        {
            return BadRequest(new { message = "Invalid signature" });
        }

        // Lấy saleId từ extraData
        try
        {
            var saleIdBytes = Convert.FromBase64String(request.ExtraData);
            var saleIdString = System.Text.Encoding.UTF8.GetString(saleIdBytes);
            var saleId = Guid.Parse(saleIdString);

            // Tìm sale
            var sale = await _dbContext.Sales.FirstOrDefaultAsync(s => s.Id == saleId);

            if (sale != null)
            {
                // Cập nhật payment status dựa vào kết quả từ Momo
                if (request.IsSuccess) // ResultCode = 0
                {
                    sale.PaymentStatus = "COMPLETED";
                    sale.Status = "COMPLETED"; // Mark sale as completed when Momo payment succeeds
                    sale.UpdatedAt = DateTime.UtcNow;
                    
                    // Update payment record if exists, otherwise create one
                    var payment = await _dbContext.Payments
                        .Where(p => p.SaleId == sale.Id && p.PaymentMethod == "MOMO")
                        .OrderByDescending(p => p.CreatedAt)
                        .FirstOrDefaultAsync();

                    if (payment == null)
                    {
                        payment = new Payment
                        {
                            Id = Guid.NewGuid(),
                            SaleId = sale.Id,
                            PaymentMethod = "MOMO",
                            Amount = (decimal)request.Amount,
                            Status = "COMPLETED",
                            TransactionReference = request.TransId.ToString(),
                            CreatedAt = DateTime.UtcNow,
                            PaymentDate = DateTime.UtcNow
                        };
                        await _dbContext.Payments.AddAsync(payment);
                    }
                    else
                    {
                        payment.Status = "COMPLETED";
                        payment.Amount = (decimal)request.Amount;
                        payment.TransactionReference = request.TransId.ToString();
                        payment.PaymentDate = DateTime.UtcNow;
                    }

                    await _dbContext.SaveChangesAsync();

                    // Reduce inventory when MOMO payment succeeds
                    try
                    {
                        var saleItems = await _dbContext.SaleItems
                            .Where(si => si.SaleId == sale.Id)
                            .ToListAsync();

                        if (saleItems.Any())
                        {
                            var inventoryItems = saleItems.Select(si => (si.ProductId, Quantity: (int)si.Quantity)).ToList();
                            var inventoryReduced = await _inventoryServiceClient.ReduceInventoryAsync(sale.StoreId, inventoryItems);

                            if (inventoryReduced)
                            {
                                _logger.LogInformation("Inventory reduced for MOMO sale {SaleNumber} after payment success", sale.SaleNumber);
                            }
                            else
                            {
                                _logger.LogWarning("Failed to reduce inventory for MOMO sale {SaleNumber}", sale.SaleNumber);
                                // Continue anyway - inventory can be manually reconciled
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error reducing inventory for MOMO sale {SaleNumber}", sale.SaleNumber);
                        // Continue anyway - don't fail the payment webhook
                    }
                }
                else
                {
                    // Thanh toán thất bại
                    sale.PaymentStatus = "FAILED";
                    sale.Status = "FAILED";
                    sale.UpdatedAt = DateTime.UtcNow;
                }

                await _dbContext.SaveChangesAsync();

                return Ok(new { message = "Success" });
            }

            return NotFound(new { message = "Sale not found" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = $"Error processing callback: {ex.Message}" });
        }
    }

    /// <summary>
    /// Redirect endpoint sau khi khách thanh toán xong trên Momo
    /// NOTE: Momo redirect URL không gửi signature. Signature verification chỉ xảy ra ở webhook (IPN callback).
    /// Redirect return URL chỉ là confirmation page - status update phụ thuộc vào webhook callback.
    /// </summary>
    /// <param name="orderId">Mã đơn hàng</param>
    /// <param name="resultCode">Mã kết quả (0 = thành công)</param>
    /// <returns>Redirect đến trang kết quả FE</returns>
    [HttpGet("momo/return")]
    public IActionResult MomoReturn(
        [FromQuery] string orderId, 
        [FromQuery] int resultCode)
    {
        var frontendSuccessUrl = _config["Momo:FrontendSuccessUrl"] ?? "http://localhost:3000/payment-success";
        var frontendFailureUrl = _config["Momo:FrontendFailureUrl"] ?? "http://localhost:3000/payment-failure";

        // ⚠️ IMPORTANT: Momo redirect result chỉ là confirmation từ user, không phải source-of-truth
        // Payment status thực sự được update qua webhook callback (MomoNotify) khi Momo server send IPN
        // Redirect result có thể bị fake, nên ta chỉ dùng để show thông báo tạm thời tới user
        
        _logger.LogInformation($"Momo redirect return - orderId: {orderId}, resultCode: {resultCode}");

        // Redirect đến FE với resultCode
        if (resultCode == 0)
        {
            // Thanh toán thành công (theo Momo redirect)
            // Nhưng payment status sẽ được confirm qua webhook callback
            _logger.LogInformation($"Momo redirect shows success for orderId: {orderId}. Waiting for webhook confirmation...");
            return Redirect($"{frontendSuccessUrl}?orderId={orderId}&resultCode={resultCode}");
        }
        else
        {
            // Thanh toán thất bại hoặc bị hủy
            _logger.LogWarning($"Momo redirect shows failure for orderId: {orderId}. ResultCode: {resultCode}");
            return Redirect($"{frontendFailureUrl}?orderId={orderId}&resultCode={resultCode}");
        }
    }

    /// <summary>
    /// Lấy danh sách payments (for testing)
    /// </summary>
    /// <returns>Danh sách tất cả payments</returns>
    [HttpGet("list")]
    [ProducesResponseType(typeof(List<Payment>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPayments()
    {
        var payments = await _dbContext.Payments
            .AsNoTracking()
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

        return Ok(payments);
    }

    /// <summary>
    /// Lấy chi tiết payment theo ID
    /// </summary>
    /// <param name="id">ID của payment</param>
    /// <returns>Thông tin payment</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(Payment), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPaymentById(Guid id)
    {
        var payment = await _dbContext.Payments
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id);

        if (payment == null)
        {
            return NotFound(new { message = "Payment not found" });
        }
        return Ok(payment);
    }

    /// <summary>
    /// 🧪 TEST ONLY - Simulate Momo payment success (không cần quét QR)
    /// </summary>
    /// <param name="saleId">ID của sale đã tạo payment</param>
    /// <returns>Kết quả simulate</returns>
    [HttpPost("momo/simulate-success/{saleId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SimulateMomoSuccess(Guid saleId)
    {
        // Tìm payment
        var payment = await _dbContext.Payments
            .Where(p => p.SaleId == saleId && p.PaymentMethod == "MOMO")
            .OrderByDescending(p => p.CreatedAt)
            .FirstOrDefaultAsync();

        if (payment == null)
        {
            return NotFound(new { message = "Payment not found. Please create payment first." });
        }

        // Tìm sale
        var sale = await _dbContext.Sales.FirstOrDefaultAsync(s => s.Id == saleId);
        if (sale == null)
        {
            return NotFound(new { message = "Sale not found" });
        }

        // Simulate payment success
        payment.Status = "COMPLETED";
        payment.TransactionReference = $"MOMO-TEST-{DateTime.UtcNow:yyyyMMddHHmmss}";
        payment.PaymentDate = DateTime.UtcNow;

        // Update sale
        sale.PaymentStatus = "COMPLETED";
        sale.Status = "COMPLETED";
        sale.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        return Ok(new
        {
            success = true,
            message = "✅ Đã simulate thanh toán thành công!",
            payment = new
            {
                paymentId = payment.Id,
                status = payment.Status,
                transactionReference = payment.TransactionReference,
                amount = payment.Amount
            },
            sale = new
            {
                saleId = sale.Id,
                saleNumber = sale.SaleNumber,
                paymentStatus = sale.PaymentStatus,
                status = sale.Status
            }
        });
    }

    /// <summary>
    /// 🧪 TEST ONLY - Simulate Momo payment failure
    /// </summary>
    /// <param name="saleId">ID của sale đã tạo payment</param>
    /// <returns>Kết quả simulate</returns>
    [HttpPost("momo/simulate-failure/{saleId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SimulateMomoFailure(Guid saleId)
    {
        var payment = await _dbContext.Payments
            .Where(p => p.SaleId == saleId && p.PaymentMethod == "MOMO")
            .OrderByDescending(p => p.CreatedAt)
            .FirstOrDefaultAsync();

        if (payment == null)
        {
            return NotFound(new { message = "Payment not found" });
        }

        var sale = await _dbContext.Sales.FirstOrDefaultAsync(s => s.Id == saleId);
        if (sale == null)
        {
            return NotFound(new { message = "Sale not found" });
        }

        // Simulate payment failure
        payment.Status = "FAILED";
        sale.PaymentStatus = "FAILED";
        sale.Status = "FAILED";
        sale.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        return Ok(new
        {
            success = false,
            message = "❌ Đã simulate thanh toán thất bại!",
            payment = new
            {
                paymentId = payment.Id,
                status = payment.Status,
                amount = payment.Amount
            }
        });
    }
}
