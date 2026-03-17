using Microsoft.AspNetCore.Mvc;
using PosService.Application.DTOs;
using PosService.Application.DTOs.External;
using PosService.Application.Interfaces;
using PosService.Application.Interfaces.Http;
using PosService.Domain.Entities;

namespace PosService.API.Controllers;

[ApiController]
[Route("api/sales")]
public class SalesController : ControllerBase
{
    private readonly ISaleRepository _saleRepository;
    private readonly IProductServiceClient _productServiceClient;
    private readonly IPromotionServiceClient _promotionServiceClient;
    private readonly ILogger<SalesController> _logger;

    public SalesController(
        ISaleRepository saleRepository,
        IProductServiceClient productServiceClient,
        IPromotionServiceClient promotionServiceClient,
        ILogger<SalesController> logger)
    {
        _saleRepository = saleRepository;
        _productServiceClient = productServiceClient;
        _promotionServiceClient = promotionServiceClient;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> CreateSale([FromBody] CreateSaleRequestDto request)
    {
        if (request == null || !request.Items.Any())
        {
            return BadRequest("Sale request cannot be empty.");
        }

        try
        {
            // 1. Re-calculate and validate the cart on the server-side
            var validationResult = await RevalidateCart(request);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.ErrorMessage);
            }

            var validatedCart = validationResult.ValidatedCart;

            if (validatedCart == null)
            {
                return BadRequest("Could not validate the cart.");
            }
            
            // 2. Check for significant price differences
            const decimal tolerance = 0.01m; // Allow for minor rounding differences
            if (Math.Abs(validatedCart.TotalAmount - request.TotalAmountFromClient) > tolerance)
            {
                _logger.LogWarning("Client total amount {ClientTotal} does not match server-calculated total {ServerTotal}. Rejecting sale.", 
                    request.TotalAmountFromClient, validatedCart.TotalAmount);
                return BadRequest("The final price has changed. Please review your cart and try again.");
            }

            // 3. Determine payment status based on payment method
            var paymentStatus = "PAID"; // Default: CASH, CARD
            var saleStatus = "COMPLETED"; // Default: CASH, CARD
            
            if (request.PaymentMethod?.ToUpper() == "MOMO")
            {
                paymentStatus = "PENDING"; // Momo payment is pending until callback
                saleStatus = "PENDING";
            }

            // 4. Create Sale and SaleItem entities using SERVER-VALIDATED data
            var sale = new Sale
            {
                Id = Guid.NewGuid(),
                StoreId = request.StoreId,
                CashierId = request.CashierId,
                CustomerId = request.CustomerId,
                SaleDate = DateTime.UtcNow,
                Subtotal = validatedCart.Subtotal,
                DiscountAmount = validatedCart.TotalDiscountAmount,
                TaxAmount = 0, // Assuming no tax for now
                TotalAmount = validatedCart.TotalAmount,
                PaymentMethod = request.PaymentMethod ?? "CASH",
                PaymentStatus = paymentStatus,
                Status = saleStatus,
                VoucherCode = request.VoucherCode,
                PointsEarned = validatedCart.PointsEarned,
                Notes = request.Notes,
                SaleItems = validatedCart.Items.Select(item => new SaleItem
                {
                    Id = Guid.NewGuid(),
                    ProductId = item.ProductId,
                    ProductName = item.ProductName,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice, // Use server-validated price
                    LineTotal = item.LineTotal,
                    DiscountAmount = item.DiscountAmount
                }).ToList()
            };

            // 5. Save to database
            var createdSale = await _saleRepository.CreateAsync(sale);

            // 6. If Momo payment, return with payment URL
            if (request.PaymentMethod?.ToUpper() == "MOMO")
            {
                return Ok(new
                {
                    saleId = createdSale.Id,
                    message = "Sale created successfully. Please proceed to Momo payment.",
                    paymentRequired = true,
                    redirectUrl = $"http://localhost:5006/api/payment/momo/create/{createdSale.Id}"
                });
            }

            // 7. Return the created sale for CASH/CARD payments
            return CreatedAtAction(nameof(GetSaleById), new { id = createdSale.Id }, createdSale);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while creating the sale.");
            return StatusCode(500, "An internal server error occurred.");
        }
    }

    [HttpPost("calculate-cart")]
    public async Task<IActionResult> CalculateCart([FromBody] CalculateCartRequestDto request)
    {
        if (request == null || !request.Items.Any())
        {
            return BadRequest("Cart items cannot be empty.");
        }

        try
        {
            // 1. Get product details from ProductService
            var productIds = request.Items.Select(i => i.ProductId).ToList();
            var productDetails = await _productServiceClient.GetProductDetailsBatchAsync(productIds);

            if (productDetails.Count != productIds.Count)
            {
                var foundIds = productDetails.Select(p => p.Id).ToList();
                var notFoundIds = productIds.Except(foundIds);
                _logger.LogWarning("Could not find product details for IDs: {NotFoundIds}", string.Join(", ", notFoundIds));
                return BadRequest($"Could not find product details for some IDs: {string.Join(", ", notFoundIds)}");
            }

            var productDetailsMap = productDetails.ToDictionary(p => p.Id);

            // 2. Prepare request for PromotionService
            var promotionRequest = new PromotionCalculationRequestDto
            {
                CustomerId = request.CustomerId,
                VoucherCode = request.VoucherCode,
                Items = request.Items.Select(item =>
                {
                    var product = productDetailsMap[item.ProductId];
                    return new PromotionCartItemDto
                    {
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        UnitPrice = product.Price,
                        CategoryId = product.CategoryId
                    };
                }).ToList()
            };

            // 3. Calculate promotions
            var promotionResult = await _promotionServiceClient.CalculatePromotionsAsync(promotionRequest);

            // 4. Map results to the final response DTO
            var response = new CalculatedCartResponseDto
            {
                Subtotal = promotionResult.Subtotal,
                TotalDiscountAmount = promotionResult.TotalDiscountAmount,
                TotalAmount = promotionResult.TotalAmount,
                PointsEarned = promotionResult.PointsEarned,
                AppliedPromotions = promotionResult.AppliedPromotions.Select(p => new Application.DTOs.AppliedPromotionDto
                {
                    PromotionId = p.PromotionId,
                    PromotionName = p.PromotionName,
                    DiscountAmount = p.DiscountAmount
                }).ToList(),
                Items = request.Items.Select(item =>
                {
                    var product = productDetailsMap[item.ProductId];
                    var lineTotal = product.Price * item.Quantity;
                    // This is a simplified discount distribution. A real system might get this from promotion service.
                    var itemDiscount = 0m;
                    return new CalculatedItemDto
                    {
                        ProductId = item.ProductId,
                        ProductName = product.Name,
                        Quantity = item.Quantity,
                        UnitPrice = product.Price,
                        LineTotal = lineTotal,
                        DiscountAmount = itemDiscount,
                        FinalLineTotal = lineTotal - itemDiscount
                    };
                }).ToList()
            };

            // Distribute the total discount proportionally across items (example logic)
            if (response.TotalDiscountAmount > 0 && response.Subtotal > 0)
            {
                foreach (var item in response.Items)
                {
                    var proportion = item.LineTotal / response.Subtotal;
                    var distributedDiscount = response.TotalDiscountAmount * proportion;
                    item.DiscountAmount = Math.Round(distributedDiscount, 2);
                    item.FinalLineTotal = item.LineTotal - item.DiscountAmount;
                }
            }

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while calculating the cart.");
            return StatusCode(500, "An internal server error occurred.");
        }
    }

    private async Task<(bool IsValid, string ErrorMessage, CalculatedCartResponseDto? ValidatedCart)> RevalidateCart(CreateSaleRequestDto request)
    {
        var productIds = request.Items.Select(i => i.ProductId).ToList();
        var productDetails = await _productServiceClient.GetProductDetailsBatchAsync(productIds);

        if (productDetails.Count != productIds.Count)
        {
            return (false, "Could not find details for all products.", null);
        }

        var productDetailsMap = productDetails.ToDictionary(p => p.Id);

        var promotionRequest = new PromotionCalculationRequestDto
        {
            CustomerId = request.CustomerId,
            VoucherCode = request.VoucherCode,
            Items = request.Items.Select(item =>
            {
                var product = productDetailsMap[item.ProductId];
                return new PromotionCartItemDto
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = product.Price, // Always use the price from the ProductService
                    CategoryId = product.CategoryId
                };
            }).ToList()
        };

        var promotionResult = await _promotionServiceClient.CalculatePromotionsAsync(promotionRequest);

        var validatedCart = new CalculatedCartResponseDto
        {
            Subtotal = promotionResult.Subtotal,
            TotalDiscountAmount = promotionResult.TotalDiscountAmount,
            TotalAmount = promotionResult.TotalAmount,
            PointsEarned = promotionResult.PointsEarned,
            AppliedPromotions = promotionResult.AppliedPromotions.Select(p => new Application.DTOs.AppliedPromotionDto
                {
                    PromotionId = p.PromotionId,
                    PromotionName = p.PromotionName,
                    DiscountAmount = p.DiscountAmount
                }).ToList(),
            Items = request.Items.Select(item =>
            {
                var product = productDetailsMap[item.ProductId];
                return new CalculatedItemDto
                {
                    ProductId = item.ProductId,
                    ProductName = product.Name,
                    Quantity = item.Quantity,
                    UnitPrice = product.Price,
                    LineTotal = product.Price * item.Quantity,
                    DiscountAmount = 0 // Will be distributed next
                };
            }).ToList()
        };
        
        if (validatedCart.TotalDiscountAmount > 0 && validatedCart.Subtotal > 0)
        {
            foreach (var item in validatedCart.Items)
            {
                var proportion = item.LineTotal / validatedCart.Subtotal;
                var distributedDiscount = validatedCart.TotalDiscountAmount * proportion;
                item.DiscountAmount = Math.Round(distributedDiscount, 2);
                item.FinalLineTotal = item.LineTotal - item.DiscountAmount;
            }
        }

        return (true, string.Empty, validatedCart);
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

    [HttpGet("{id}")]
    public async Task<IActionResult> GetSaleById(Guid id)
    {
        var sale = await _saleRepository.GetByIdAsync(id);
        if (sale == null)
        {
            return NotFound();
        }
        return Ok(sale);
    }
}
