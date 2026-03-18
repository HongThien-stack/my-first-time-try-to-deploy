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

        [HttpGet]
        public async Task<IActionResult> GetPromotions([FromQuery] GetPromotionsQueryDto query, CancellationToken cancellationToken)
        {
            var result = await _promotionEngineService.GetPromotionsAsync(query, cancellationToken);
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreatePromotionRequestDto request, CancellationToken cancellationToken)
        {
            try
            {
                var createdPromotion = await _promotionEngineService.CreatePromotionAsync(request, cancellationToken);
                return Created($"/api/promotions/{createdPromotion.Id}", createdPromotion);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePromotionRequestDto request, CancellationToken cancellationToken)
        {
            try
            {
                var updatedPromotion = await _promotionEngineService.UpdatePromotionAsync(id, request, cancellationToken);
                return Ok(updatedPromotion);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new
                {
                    message = ex.Message
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
        {
            try
            {
                var result = await _promotionEngineService.DeletePromotionAsync(id, cancellationToken);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new
                {
                    message = ex.Message
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
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
