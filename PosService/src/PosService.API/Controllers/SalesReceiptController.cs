using Microsoft.AspNetCore.Mvc;
using PosService.Application.DTOs;
using PosService.Application.Interfaces;
using System.Net.Mail;
using System.Text.RegularExpressions;

namespace PosService.API.Controllers;

[ApiController]
[Route("api/sales")]
public class SalesReceiptController : ControllerBase
{
    private static readonly Regex SmsRegex = new("^\\+?[1-9]\\d{8,14}$", RegexOptions.Compiled);
    private readonly IReceiptService _receiptService;
    private readonly IPdfReceiptService _pdfReceiptService;
    private readonly INotificationService _notificationService;
    private readonly ILogger<SalesReceiptController> _logger;

    public SalesReceiptController(
        IReceiptService receiptService,
        IPdfReceiptService pdfReceiptService,
        INotificationService notificationService,
        ILogger<SalesReceiptController> logger)
    {
        _receiptService = receiptService;
        _pdfReceiptService = pdfReceiptService;
        _notificationService = notificationService;
        _logger = logger;
    }

    [HttpGet("{id:guid}/receipt")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetReceiptBySaleId(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var receipt = await _receiptService.GetReceiptAsync(id, cancellationToken);
            if (receipt == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = $"Sale with id '{id}' was not found"
                });
            }

            return Ok(receipt);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while getting receipt for sale {SaleId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                success = false,
                message = "An unexpected error occurred while getting receipt"
            });
        }
    }

    [HttpGet("{id:guid}/receipt/pdf")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetReceiptPdfBySaleId(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var receipt = await _receiptService.GetReceiptAsync(id, cancellationToken);
            if (receipt == null)
            {
                return NotFound();
            }

            var pdf = await _pdfReceiptService.GenerateReceiptPdfAsync(receipt, cancellationToken);
            var fileName = $"receipt-{receipt.SaleNumber}.pdf";
            return File(pdf, "application/pdf", fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while generating receipt PDF for sale {SaleId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                success = false,
                message = "An unexpected error occurred while generating receipt PDF"
            });
        }
    }

    [HttpPost("{id:guid}/send-receipt")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SendReceiptBySaleId(
        Guid id,
        [FromBody] SendReceiptRequestDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            if (!ModelState.IsValid || request == null)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Method and destination are required"
                });
            }

            var method = request.Method.Trim().ToUpperInvariant();
            var destination = request.Destination.Trim();

            if (method != "EMAIL" && method != "SMS")
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Method must be EMAIL or SMS"
                });
            }

            if (method == "EMAIL")
            {
                try
                {
                    _ = new MailAddress(destination);
                }
                catch
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Destination must be a valid email address when method is EMAIL"
                    });
                }
            }

            if (method == "SMS" && !SmsRegex.IsMatch(destination))
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Destination must be a valid phone number when method is SMS"
                });
            }

            var receipt = await _receiptService.GetReceiptAsync(id, cancellationToken);
            if (receipt == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = $"Sale with id '{id}' was not found"
                });
            }

            byte[]? attachment = null;
            if (method == "EMAIL")
            {
                attachment = await _pdfReceiptService.GenerateReceiptPdfAsync(receipt, cancellationToken);
            }

            await _notificationService.SendReceiptAsync(method, destination, receipt, attachment, cancellationToken);

            _logger.LogInformation(
                "Receipt sent. SaleId: {SaleId}, Method: {Method}, Destination: {Destination}",
                id,
                method,
                destination);

            return Ok(new
            {
                success = true,
                message = $"Receipt {receipt.SaleNumber} has been sent via {method}",
                saleId = receipt.SaleId,
                method,
                destination
            });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Business validation failed when sending receipt for sale {SaleId}", id);
            return BadRequest(new
            {
                success = false,
                message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while sending receipt for sale {SaleId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                success = false,
                message = "An unexpected error occurred while sending receipt"
            });
        }
    }
}
