using Microsoft.AspNetCore.Mvc;
using PosService.Application.DTOs;
using PosService.Application.Interfaces;

namespace PosService.API.Controllers;

[ApiController]
[Route("api/sales")]
public class SalesController : ControllerBase
{
    private readonly ISaleRepository _saleRepository;

    public SalesController(ISaleRepository saleRepository)
    {
        _saleRepository = saleRepository;
    }

    [HttpGet]
    public async Task<IActionResult> GetSales(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] Guid? storeId = null,
        [FromQuery] Guid? cashierId = null,
        [FromQuery] string? paymentMethod = null,
        [FromQuery] string? status = null,
        [FromQuery] DateTime? dateFrom = null,
        [FromQuery] DateTime? dateTo = null)
    {
        // Validate pagination
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 10;
        if (pageSize > 100) pageSize = 100;

        var (items, totalCount) = await _saleRepository.GetAllAsync(
            page, pageSize, storeId, cashierId, paymentMethod, status, dateFrom, dateTo);

        var saleDtos = items.Select(s => new SaleDto
        {
            Id = s.Id,
            SaleNumber = s.SaleNumber,
            StoreId = s.StoreId,
            CashierId = s.CashierId,
            CustomerId = s.CustomerId,
            SaleDate = s.SaleDate,
            Subtotal = s.Subtotal,
            TaxAmount = s.TaxAmount,
            DiscountAmount = s.DiscountAmount,
            TotalAmount = s.TotalAmount,
            PaymentMethod = s.PaymentMethod,
            PaymentStatus = s.PaymentStatus,
            Status = s.Status,
            PromotionId = s.PromotionId,
            VoucherCode = s.VoucherCode,
            PointsUsed = s.PointsUsed,
            PointsEarned = s.PointsEarned,
            Notes = s.Notes,
            CreatedAt = s.CreatedAt,
            ItemCount = s.SaleItems?.Count ?? 0
        }).ToList();

        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        var response = new PaginatedResponseDto<SaleDto>
        {
            Data = saleDtos,
            Pagination = new PaginationMetadata
            {
                CurrentPage = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = totalPages
            }
        };

        return Ok(response);
    }
}
