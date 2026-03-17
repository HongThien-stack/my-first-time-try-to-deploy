namespace PromotionService.Application.DTOs
{
    public class CartDto
    {
        public Guid? CustomerId { get; set; }
        public string? VoucherCode { get; set; }
        public List<CartItemDto> Items { get; set; } = new List<CartItemDto>();
    }
}
