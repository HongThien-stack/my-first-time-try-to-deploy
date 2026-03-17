using Microsoft.AspNetCore.Mvc;
using PosService.Application.DTOs;
using PosService.Application.Interfaces;
namespace PosService.API.Controllers;

[ApiController]
[Route("api/sales")]
public class SalesReceiptController : ControllerBase
{
    private readonly IReceiptService _receiptService;
    private readonly IPdfReceiptService _pdfReceiptService;
    private readonly ILogger<SalesReceiptController> _logger;

    public SalesReceiptController(
        IReceiptService receiptService,
        IPdfReceiptService pdfReceiptService,
        ILogger<SalesReceiptController> logger)
    {
        _receiptService = receiptService;
        _pdfReceiptService = pdfReceiptService;
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

}
