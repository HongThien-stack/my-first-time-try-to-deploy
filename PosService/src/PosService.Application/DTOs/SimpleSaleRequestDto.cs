namespace PosService.Application.DTOs
{
    /// <summary>
    /// Simple Sale Request - No promotions, vouchers, discounts
    /// Just basic cart with product quantity
    /// </summary>
    public class SimpleSaleRequestDto
    {
        public Guid StoreId { get; set; }
        public Guid CashierId { get; set; }
        public Guid? CustomerId { get; set; }
        public List<SimpleSaleItemDto> Items { get; set; } = new();
        public string PaymentMethod { get; set; } = "CASH"; // CASH or MOMO
        public string? Notes { get; set; }
    }

    public class SimpleSaleItemDto
    {
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
    }
}
