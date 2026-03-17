using Microsoft.AspNetCore.Mvc;
using PromotionService.Application.DTOs;
using PromotionService.Application.Interfaces;

namespace PromotionService.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PromotionsController : ControllerBase
    {
        private readonly IPromotionEngineService _promotionEngineService;

        public PromotionsController(IPromotionEngineService promotionEngineService)
        {
            _promotionEngineService = promotionEngineService;
        }

        [HttpPost("calculate")]
        public async Task<IActionResult> Calculate([FromBody] CartDto cart)
        {
            if (cart == null || !cart.Items.Any())
            {
                return BadRequest("Cart cannot be null or empty.");
            }

            var result = await _promotionEngineService.CalculateDiscountsAsync(cart);
            return Ok(result);
        }
    }
}
