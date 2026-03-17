namespace PosService.Application.DTOs
{
    public class CalculateCartRequestDto
    {
        public Guid? CustomerId { get; set; }
        public string? VoucherCode { get; set; }
        public List<CartItemRequestDto> Items { get; set; } = new();
    }

    public class CartItemRequestDto
    {
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
    }
}
