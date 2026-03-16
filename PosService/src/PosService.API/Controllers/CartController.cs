using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PosService.Application.DTOs;
using PosService.Application.Services;

namespace PosService.API.Controllers;

[ApiController]
[Route("api/cart")]
[Authorize(Roles = "Store Staff")]
public class CartController : ControllerBase
{
    private readonly CartService _cartService;

    public CartController(CartService cartService)
    {
        _cartService = cartService;
    }

    /// <summary>
    /// POST /api/cart/create - Tạo giỏ hàng mới (bắt đầu giao dịch)
    /// </summary>
    [HttpPost("create")]
    public async Task<IActionResult> CreateCart([FromBody] CreateCartDto dto)
    {
        try
        {
            var cart = await _cartService.CreateCartAsync(dto);

            return Ok(new
            {
                success = true,
                message = "Cart created successfully",
                data = cart
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
            var cart = await _cartService.GetCartByIdAsync(cartId);

            return Ok(new
            {
                success = true,
                data = cart
            });
        }
        catch (Exception ex)
        {
            return NotFound(new
            {
                success = false,
                message = ex.Message
            });
        }
    }

    /// <summary>
    /// POST /api/cart/{cartId}/items - Thêm sản phẩm vào giỏ hàng
    /// </summary>
    [HttpPost("{cartId}/items")]
    public async Task<IActionResult> AddItemToCart(Guid cartId, [FromBody] AddCartItemDto dto)
    {
        try
        {
            var authHeader = Request.Headers["Authorization"].FirstOrDefault();
            var cart = await _cartService.AddItemToCartAsync(cartId, dto, authHeader);

            return Ok(new
            {
                success = true,
                message = "Item added to cart successfully",
                data = cart
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
    /// PUT /api/cart/{cartId}/items/{itemId} - Cập nhật số lượng sản phẩm
    /// </summary>
    [HttpPut("{cartId}/items/{itemId}")]
    public async Task<IActionResult> UpdateCartItem(Guid cartId, Guid itemId, [FromBody] UpdateCartItemDto dto)
    {
        try
        {
            var cart = await _cartService.UpdateCartItemAsync(cartId, itemId, dto);

            return Ok(new
            {
                success = true,
                message = "Item quantity updated successfully",
                data = cart
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
    /// DELETE /api/cart/{cartId}/items/{itemId} - Xóa sản phẩm khỏi giỏ hàng
    /// </summary>
    [HttpDelete("{cartId}/items/{itemId}")]
    public async Task<IActionResult> RemoveCartItem(Guid cartId, Guid itemId)
    {
        try
        {
            await _cartService.RemoveItemFromCartAsync(cartId, itemId);
            var cart = await _cartService.GetCartByIdAsync(cartId);

            return Ok(new
            {
                success = true,
                message = "Item removed from cart successfully",
                data = cart
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
    /// POST /api/cart/{cartId}/complete - Hoàn thành thanh toán
    /// </summary>
    [HttpPost("{cartId}/complete")]
    public async Task<IActionResult> CompleteCart(Guid cartId)
    {
        try
        {
            var cart = await _cartService.CompleteCartAsync(cartId);

            return Ok(new
            {
                success = true,
                message = "Cart completed successfully. Payment saved.",
                data = cart
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
