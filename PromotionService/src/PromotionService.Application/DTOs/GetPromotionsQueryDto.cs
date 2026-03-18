namespace PromotionService.Application.DTOs
{
    public class GetPromotionsQueryDto
    {
        public bool? IsActive { get; set; }
        public string? PromotionType { get; set; }
    }
}
