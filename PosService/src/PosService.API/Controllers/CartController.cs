using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PosService.Application.DTOs;
using PosService.Application.Interfaces;

namespace PosService.API.Controllers;

[ApiController]
[Route("api/cart")]
[Authorize(Roles = "Store Staff")]
public class CartController : ControllerBase
{
    private readonly ISaleRepository _saleRepository;

    public CartController(ISaleRepository saleRepository)
    {
        _saleRepository = saleRepository;
    }

    /// <summary>
    /// POST /api/cart/create - Tạo giỏ hàng mới (bắt đầu giao dịch)
    /// </summary>
    [HttpPost("create")]
    public async Task<IActionResult> CreateCart([FromBody] CreateCartDto dto)
    {
        try
        {
            var cart = await _saleRepository.CreateCartAsync(
                dto.StoreId,
                dto.CashierId,
                dto.CustomerId,
                dto.Notes
            );

            var cartDto = new CartDto
            {
                Id = cart.Id,
                SaleNumber = cart.SaleNumber,
                StoreId = cart.StoreId,
                CashierId = cart.CashierId,
                CustomerId = cart.CustomerId,
                Subtotal = cart.Subtotal,
                TaxAmount = cart.TaxAmount,
                DiscountAmount = cart.DiscountAmount,
                TotalAmount = cart.TotalAmount,
                Status = cart.Status,
                PaymentStatus = cart.PaymentStatus,
                PaymentMethod = cart.PaymentMethod,
                Notes = cart.Notes,
                CreatedAt = cart.CreatedAt,
                UpdatedAt = cart.UpdatedAt,
                Items = new List<CartItemDto>()
            };

            return Ok(new
            {
                success = true,
                message = "Cart created successfully",
                data = cartDto
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new
            {
                success = false,
                message = ex.Message
            });
        }
    }

    /// <summary>
    /// GET /api/cart/{cartId} - Xem chi tiết giỏ hàng + items
    /// </summary>
    [HttpGet("{cartId}")]
    public async Task<IActionResult> GetCart(Guid cartId)
    {
        try
        {
            var cart = await _saleRepository.GetCartByIdAsync(cartId);
            
            if (cart == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Cart not found or already completed"
                });
            }

            var cartDto = new CartDto
            {
                Id = cart.Id,
                SaleNumber = cart.SaleNumber,
                StoreId = cart.StoreId,
                CashierId = cart.CashierId,
                CustomerId = cart.CustomerId,
                Subtotal = cart.Subtotal,
                TaxAmount = cart.TaxAmount,
                DiscountAmount = cart.DiscountAmount,
                TotalAmount = cart.TotalAmount,
                Status = cart.Status,
                PaymentStatus = cart.PaymentStatus,
                PaymentMethod = cart.PaymentMethod,
                Notes = cart.Notes,
                CreatedAt = cart.CreatedAt,
                UpdatedAt = cart.UpdatedAt,
                Items = cart.SaleItems.Select(item => new CartItemDto
                {
                    Id = item.Id,
                    ProductId = item.ProductId,
                    ProductName = item.ProductName,
                    ProductSku = item.ProductSku,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    LineDiscount = item.LineDiscount,
                    LineTotal = item.LineTotal
                }).ToList()
            };

            return Ok(new
            {
                success = true,
                data = cartDto
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new
            {
                success = false,
                message = ex.Message
            });
        }
    }

    /// <summary>
    /// POST /api/cart/{cartId}/items - Thêm sản phẩm (quét barcode)
    /// </summary>
    [HttpPost("{cartId}/items")]
    public async Task<IActionResult> AddItemToCart(Guid cartId, [FromBody] AddCartItemDto dto)
    {
        try
        {
            var item = await _saleRepository.AddItemToCartAsync(cartId, dto.Barcode, dto.Quantity);
            
            // Get updated cart
            var cart = await _saleRepository.GetCartByIdAsync(cartId);

            var cartDto = new CartDto
            {
                Id = cart!.Id,
                SaleNumber = cart.SaleNumber,
                StoreId = cart.StoreId,
                CashierId = cart.CashierId,
                CustomerId = cart.CustomerId,
                Subtotal = cart.Subtotal,
                TaxAmount = cart.TaxAmount,
                DiscountAmount = cart.DiscountAmount,
                TotalAmount = cart.TotalAmount,
                Status = cart.Status,
                PaymentStatus = cart.PaymentStatus,
                PaymentMethod = cart.PaymentMethod,
                Notes = cart.Notes,
                CreatedAt = cart.CreatedAt,
                UpdatedAt = cart.UpdatedAt,
                Items = cart.SaleItems.Select(i => new CartItemDto
                {
                    Id = i.Id,
                    ProductId = i.ProductId,
                    ProductName = i.ProductName,
                    ProductSku = i.ProductSku,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice,
                    LineDiscount = i.LineDiscount,
                    LineTotal = i.LineTotal
                }).ToList()
            };

            return Ok(new
            {
                success = true,
                message = "Item added to cart successfully",
                data = cartDto
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new
            {
                success = false,
                message = ex.Message
            });
        }
    }

    /// <summary>
    /// PUT /api/cart/{cartId}/items/{itemId} - Sửa số lượng
    /// </summary>
    [HttpPut("{cartId}/items/{itemId}")]
    public async Task<IActionResult> UpdateCartItem(Guid cartId, Guid itemId, [FromBody] UpdateCartItemDto dto)
    {
        try
        {
            var item = await _saleRepository.UpdateCartItemAsync(cartId, itemId, dto.Quantity);
            
            if (item == null && dto.Quantity > 0)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Cart or item not found"
                });
            }

            // Get updated cart
            var cart = await _saleRepository.GetCartByIdAsync(cartId);

            var cartDto = new CartDto
            {
                Id = cart!.Id,
                SaleNumber = cart.SaleNumber,
                StoreId = cart.StoreId,
                CashierId = cart.CashierId,
                CustomerId = cart.CustomerId,
                Subtotal = cart.Subtotal,
                TaxAmount = cart.TaxAmount,
                DiscountAmount = cart.DiscountAmount,
                TotalAmount = cart.TotalAmount,
                Status = cart.Status,
                PaymentStatus = cart.PaymentStatus,
                PaymentMethod = cart.PaymentMethod,
                Notes = cart.Notes,
                CreatedAt = cart.CreatedAt,
                UpdatedAt = cart.UpdatedAt,
                Items = cart.SaleItems.Select(i => new CartItemDto
                {
                    Id = i.Id,
                    ProductId = i.ProductId,
                    ProductName = i.ProductName,
                    ProductSku = i.ProductSku,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice,
                    LineDiscount = i.LineDiscount,
                    LineTotal = i.LineTotal
                }).ToList()
            };

            return Ok(new
            {
                success = true,
                message = "Item quantity updated successfully",
                data = cartDto
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new
            {
                success = false,
                message = ex.Message
            });
        }
    }

    /// <summary>
    /// DELETE /api/cart/{cartId}/items/{itemId} - Xóa sản phẩm
    /// </summary>
    [HttpDelete("{cartId}/items/{itemId}")]
    public async Task<IActionResult> RemoveCartItem(Guid cartId, Guid itemId)
    {
        try
        {
            var result = await _saleRepository.RemoveCartItemAsync(cartId, itemId);
            
            if (!result)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Cart or item not found"
                });
            }

            // Get updated cart
            var cart = await _saleRepository.GetCartByIdAsync(cartId);

            var cartDto = new CartDto
            {
                Id = cart!.Id,
                SaleNumber = cart.SaleNumber,
                StoreId = cart.StoreId,
                CashierId = cart.CashierId,
                CustomerId = cart.CustomerId,
                Subtotal = cart.Subtotal,
                TaxAmount = cart.TaxAmount,
                DiscountAmount = cart.DiscountAmount,
                TotalAmount = cart.TotalAmount,
                Status = cart.Status,
                PaymentStatus = cart.PaymentStatus,
                PaymentMethod = cart.PaymentMethod,
                Notes = cart.Notes,
                CreatedAt = cart.CreatedAt,
                UpdatedAt = cart.UpdatedAt,
                Items = cart.SaleItems.Select(i => new CartItemDto
                {
                    Id = i.Id,
                    ProductId = i.ProductId,
                    ProductName = i.ProductName,
                    ProductSku = i.ProductSku,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice,
                    LineDiscount = i.LineDiscount,
                    LineTotal = i.LineTotal
                }).ToList()
            };

            return Ok(new
            {
                success = true,
                message = "Item removed from cart successfully",
                data = cartDto
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new
            {
                success = false,
                message = ex.Message
            });
        }
    }
}
