using Microsoft.AspNetCore.Mvc;
using PosService.Application.DTOs;
using PosService.Application.DTOs.External;
using PosService.Application.Interfaces;
using PosService.Application.Interfaces.Http;
using PosService.Domain.Entities;
using PosService.Infrastructure.Data;

namespace PosService.API.Controllers;

[ApiController]
[Route("api/sales")]
public class SalesController : ControllerBase
{
    private readonly ISaleRepository _saleRepository;
    private readonly IProductServiceClient _productServiceClient;
    private readonly IPromotionServiceClient _promotionServiceClient;
    private readonly IInventoryServiceClient _inventoryServiceClient;
    private readonly IMomoPaymentService _momoService;
    private readonly ILogger<SalesController> _logger;
    private readonly PosDbContext _dbContext;

    public SalesController(
        ISaleRepository saleRepository,
        IProductServiceClient productServiceClient,
        IPromotionServiceClient promotionServiceClient,
        IInventoryServiceClient inventoryServiceClient,
        IMomoPaymentService momoService,
        ILogger<SalesController> logger,
        PosDbContext dbContext)
    {
        _saleRepository = saleRepository;
        _productServiceClient = productServiceClient;
        _promotionServiceClient = promotionServiceClient;
        _inventoryServiceClient = inventoryServiceClient;
        _momoService = momoService;
        _logger = logger;
        _dbContext = dbContext;
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
            
            // 2. Check for price differences and prefer server-calculated totals.
            // This avoids blocking checkout when client-side totals are stale.
            const decimal tolerance = 0.01m; // Allow minor rounding differences
            if (Math.Abs(validatedCart.TotalAmount - request.TotalAmountFromClient) > tolerance)
            {
                _logger.LogWarning("Client total amount {ClientTotal} does not match server-calculated total {ServerTotal}. Proceeding with server total.",
                    request.TotalAmountFromClient, validatedCart.TotalAmount);
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
                    var categoryId = product.CategoryId.ToString();
                    _logger.LogInformation("CalculateCart - Product: {ProductName}, CategoryId: {CategoryId}", product.Name, categoryId);
                    return new PromotionCartItemDto
                    {
                        ProductId = item.ProductId,
                        ProductName = product.Name,
                        Quantity = item.Quantity,
                        UnitPrice = product.Price,
                        CategoryId = product.CategoryId,
                        Categories = string.IsNullOrEmpty(categoryId) 
                            ? new List<string>() 
                            : new List<string> { categoryId }
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

    /// <summary>
    /// Create a simple sale without promotions, vouchers, or discounts
    /// Just basic cart calculation with CASH or MOMO payment
    /// </summary>
    [HttpPost("simple")]
    public async Task<IActionResult> CreateSimpleSale([FromBody] SimpleSaleRequestDto request)
    {
        if (request == null || !request.Items.Any())
        {
            return BadRequest("Sale items cannot be empty.");
        }

        try
        {
            // 1. Validate payment method
            if (!new[] { "CASH", "MOMO" }.Contains(request.PaymentMethod?.ToUpper()))
            {
                return BadRequest("Payment method must be CASH or MOMO.");
            }

            // 2. Get product details
            var productIds = request.Items.Select(i => i.ProductId).ToList();
            var productDetails = await _productServiceClient.GetProductDetailsBatchAsync(productIds);

            if (productDetails.Count != productIds.Count)
            {
                var foundIds = productDetails.Select(p => p.Id).ToList();
                var notFoundIds = productIds.Except(foundIds);
                return BadRequest($"Could not find products: {string.Join(", ", notFoundIds)}");
            }

            var productDetailsMap = productDetails.ToDictionary(p => p.Id);

            // 2.5. Check inventory availability BEFORE creating sale
            var inventoryCheckItems = request.Items.Select(i => (i.ProductId, i.Quantity)).ToList();
            var inventoryCheck = await _inventoryServiceClient.CheckAvailabilityAsync(request.StoreId, inventoryCheckItems);
            
            if (!inventoryCheck.IsAvailable)
            {
                // Build error message with product names and quantities
                var unavailableProducts = string.Join(", ", 
                    inventoryCheck.UnavailableItems.Select(u => 
                        $"{u.ProductName} (need: {u.RequestedQty}, available: {u.AvailableQty})"));
                
                _logger.LogWarning("Insufficient inventory for sale at store {StoreId}. Unavailable: {UnavailableProducts}", 
                    request.StoreId, unavailableProducts);
                
                return BadRequest(new { 
                    success = false,
                    message = "Insufficient inventory for some items",
                    unavailableProducts = unavailableProducts,
                    details = inventoryCheck.UnavailableItems
                });
            }

            // 3. Calculate totals (no promotions)
            decimal subtotal = 0;
            var saleItems = new List<SaleItem>();

            foreach (var item in request.Items)
            {
                var product = productDetailsMap[item.ProductId];
                var lineTotal = product.Price * item.Quantity;
                subtotal += lineTotal;

                saleItems.Add(new SaleItem
                {
                    Id = Guid.NewGuid(),
                    ProductId = item.ProductId,
                    ProductName = product.Name,
                    Sku = product.Sku ?? string.Empty,
                    Quantity = item.Quantity,
                    UnitPrice = product.Price,
                    DiscountAmount = 0,
                    LineTotal = lineTotal,
                    PromotionApplied = false
                });
            }

            // 4. Generate sale number
            var saleNumber = $"SALE-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}";

            // 5. Create Sale
            var paymentMethod = request.PaymentMethod?.ToUpper() ?? "CASH";
            var paymentStatus = paymentMethod == "MOMO" ? "PENDING" : "PAID";
            var saleStatus = paymentMethod == "MOMO" ? "PENDING" : "COMPLETED";

            var sale = new Sale
            {
                Id = Guid.NewGuid(),
                SaleNumber = saleNumber,
                StoreId = request.StoreId,
                CashierId = request.CashierId,
                CustomerId = request.CustomerId,
                SaleDate = DateTime.UtcNow,
                Subtotal = subtotal,
                DiscountAmount = 0,
                TaxAmount = 0,
                TotalAmount = subtotal,
                PaymentMethod = paymentMethod,
                PaymentStatus = paymentStatus,
                Status = saleStatus,
                PointsEarned = 0,
                PointsUsed = 0,
                Notes = request.Notes,
                SaleItems = saleItems,
                CreatedAt = DateTime.UtcNow
            };

            // 6. Save Sale
            var createdSale = await _saleRepository.CreateAsync(sale);

            // 7. Create Payment record
            var payment = new Payment
            {
                Id = Guid.NewGuid(),
                SaleId = createdSale.Id,
                PaymentMethod = paymentMethod,
                Amount = subtotal,
                Status = paymentStatus,
                PaymentDate = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };

            _dbContext.Payments.Add(payment);
            await _dbContext.SaveChangesAsync();

            // 7.5. For CASH payment, reduce inventory immediately
            if (paymentMethod == "CASH")
            {
                var inventoryItems = request.Items.Select(i => (i.ProductId, i.Quantity)).ToList();
                var inventoryReduced = await _inventoryServiceClient.ReduceInventoryAsync(request.StoreId, inventoryItems);

                if (!inventoryReduced)
                {
                    _logger.LogWarning("Failed to reduce inventory for store {StoreId}, sale {SaleNumber}", 
                        request.StoreId, createdSale.SaleNumber);
                    // Continue anyway - inventory can be manually reconciled
                }
                else
                {
                    _logger.LogInformation("Inventory reduced for CASH sale {SaleNumber}", createdSale.SaleNumber);
                }
            }

            // 8. If MOMO, call Momo API to generate payment link
            if (paymentMethod == "MOMO")
            {
                try
                {
                    // Call Momo API to create payment
                    var momoResponse = await _momoService.CreatePaymentAsync(
                        saleNumber: createdSale.SaleNumber,
                        amount: subtotal,
                        saleId: createdSale.Id,
                        paymentType: "WALLET"  // QR Code payment
                    );

                    if (momoResponse.IsSuccess)
                    {
                        // Update payment with Momo transaction info
                        payment.TransactionReference = momoResponse.RequestId;
                        _dbContext.Payments.Update(payment);
                        await _dbContext.SaveChangesAsync();

                        var response = new SimpleSaleResponseDto
                        {
                            SaleId = createdSale.Id,
                            SaleNumber = createdSale.SaleNumber,
                            Subtotal = subtotal,
                            TotalAmount = subtotal,
                            PaymentMethod = paymentMethod,
                            Status = saleStatus,
                            PaymentStatus = paymentStatus,
                            SaleDate = createdSale.SaleDate,
                            PaymentId = payment.Id,
                            MomoPayUrl = momoResponse.PayUrl,          // Real Momo URL
                            MomoQrUrl = momoResponse.QrCodeUrl,        // Real QR URL  
                            Items = request.Items.Select(item =>
                            {
                                var product = productDetailsMap[item.ProductId];
                                return new SimpleSaleItemResponseDto
                                {
                                    ProductId = item.ProductId,
                                    ProductName = product.Name,
                                    Quantity = item.Quantity,
                                    UnitPrice = product.Price,
                                    LineTotal = product.Price * item.Quantity
                                };
                            }).ToList()
                        };

                        _logger.LogInformation("SimpleSale Momo created: {SaleNumber}, RequestId: {RequestId}", 
                            createdSale.SaleNumber, momoResponse.RequestId);

                        return Accepted(response);
                    }
                    else
                    {
                        _logger.LogError("Momo API call failed: {Message} (Code: {Code})", 
                            momoResponse.Message, momoResponse.ResultCode);
                        return BadRequest(new { message = $"Momo payment creation failed: {momoResponse.Message}" });
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error calling Momo API for sale {SaleNumber}", createdSale.SaleNumber);
                    return StatusCode(500, "Error creating Momo payment. Please try again.");
                }
            }

            // 9. Return response for CASH payment
            return Ok(new SimpleSaleResponseDto
            {
                SaleId = createdSale.Id,
                SaleNumber = createdSale.SaleNumber,
                Subtotal = subtotal,
                TotalAmount = subtotal,
                PaymentMethod = paymentMethod,
                Status = saleStatus,
                PaymentStatus = paymentStatus,
                SaleDate = createdSale.SaleDate,
                Items = request.Items.Select(item =>
                {
                    var product = productDetailsMap[item.ProductId];
                    return new SimpleSaleItemResponseDto
                    {
                        ProductId = item.ProductId,
                        ProductName = product.Name,
                        Quantity = item.Quantity,
                        UnitPrice = product.Price,
                        LineTotal = product.Price * item.Quantity
                    };
                }).ToList()
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while creating a simple sale.");
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
                    ProductName = product.Name,
                    Quantity = item.Quantity,
                    UnitPrice = product.Price,
                    CategoryId = product.CategoryId,
                    Categories = string.IsNullOrEmpty(product.CategoryId.ToString()) 
                        ? new List<string>() 
                        : new List<string> { product.CategoryId.ToString() }
                };
            }).ToList()
        };

        var promotionResult = await _promotionServiceClient.CalculatePromotionsAsync(promotionRequest);

        _logger.LogInformation("PromotionService returned - Subtotal: {Subtotal}, Discount: {Discount}, Total: {Total}", 
            promotionResult.Subtotal, promotionResult.TotalDiscountAmount, promotionResult.TotalAmount);

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
            TotalAmount = s.TotalAmount,
            PaymentMethod = s.PaymentMethod,
            PaymentStatus = s.PaymentStatus,
            Status = s.Status,
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

        var saleDetailDto = new SaleDetailDto
        {
            Id = sale.Id,
            SaleNumber = sale.SaleNumber,
            StoreId = sale.StoreId,
            CashierId = sale.CashierId,
            CustomerId = sale.CustomerId,
            SaleDate = sale.SaleDate,
            Subtotal = sale.Subtotal,
            TotalAmount = sale.TotalAmount,
            PaymentMethod = sale.PaymentMethod,
            PaymentStatus = sale.PaymentStatus,
            Status = sale.Status,
            Notes = sale.Notes,
            CreatedAt = sale.CreatedAt,
            Items = sale.SaleItems?.Select(si => new SaleItemDetailDto
            {
                Id = si.Id,
                ProductId = si.ProductId,
                ProductName = si.ProductName,
                Sku = si.Sku,
                Quantity = si.Quantity,
                UnitPrice = si.UnitPrice,
                LineTotal = si.LineTotal
            }).ToList() ?? new(),
            Payments = sale.Payments?.Select(p => new PaymentDetailDto
            {
                Id = p.Id,
                PaymentMethod = p.PaymentMethod,
                Amount = p.Amount,
                Status = p.Status,
                CashReceived = p.CashReceived,
                CashChange = p.CashChange,
                TransactionReference = p.TransactionReference,
                PaymentDate = p.PaymentDate,
                CreatedAt = p.CreatedAt
            }).ToList() ?? new()
        };

        return Ok(saleDetailDto);
    }
}
