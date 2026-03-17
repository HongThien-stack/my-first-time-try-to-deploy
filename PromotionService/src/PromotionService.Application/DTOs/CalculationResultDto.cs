namespace PromotionService.Application.DTOs
{
    public class CalculationResultDto
    {
        public decimal Subtotal { get; set; }
        public decimal TotalDiscountAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public int PointsEarned { get; set; }
        public List<AppliedPromotionDto> AppliedPromotions { get; set; } = new List<AppliedPromotionDto>();
        public List<CalculatedCartItemDto> Items { get; set; } = new List<CalculatedCartItemDto>();
    }

    public class AppliedPromotionDto
    {
        public Guid PromotionId { get; set; }
        public string PromotionName { get; set; } = string.Empty;
        public decimal DiscountAmount { get; set; }
    }

    public class CalculatedCartItemDto
    {
        public Guid ProductId { get; set; }
        public decimal LineTotal { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal FinalLineTotal { get; set; }
    }
}
