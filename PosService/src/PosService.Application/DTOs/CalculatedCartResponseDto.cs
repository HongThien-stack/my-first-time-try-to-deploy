namespace PosService.Application.DTOs
{
    // DTO for the final calculated cart response
    public class CalculatedCartResponseDto
    {
        public decimal Subtotal { get; set; }
        public decimal TotalDiscountAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public int PointsEarned { get; set; }
        public List<CalculatedItemDto> Items { get; set; } = new();
        public List<AppliedPromotionDto> AppliedPromotions { get; set; } = new();
    }

    // DTO for each item in the calculated cart
    public class CalculatedItemDto
    {
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal LineTotal { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal FinalLineTotal { get; set; }
    }

    // DTO for applied promotions
    public class AppliedPromotionDto
    {
        public Guid PromotionId { get; set; }
        public string PromotionName { get; set; } = string.Empty;
        public decimal DiscountAmount { get; set; }
    }
}
