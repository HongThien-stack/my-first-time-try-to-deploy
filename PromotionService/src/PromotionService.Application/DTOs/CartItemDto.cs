namespace PromotionService.Application.DTOs
{
    public class CartItemDto
    {
        public Guid ProductId { get; set; }
        public Guid CategoryId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }
}
