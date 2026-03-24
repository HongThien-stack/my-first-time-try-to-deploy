namespace PosService.Application.DTOs
{
    public class CreateSaleRequestDto
    {
        public Guid StoreId { get; set; }
        public Guid CashierId { get; set; }
        public Guid? CustomerId { get; set; }
        public List<CreateSaleItemDto> Items { get; set; } = new();
        public string PaymentMethod { get; set; } = "CASH";
        public decimal TotalAmountFromClient { get; set; } // The total amount calculated by the client
        public string? VoucherCode { get; set; }
        public string? Notes { get; set; }
    }

    public class CreateSaleItemDto
    {
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
    }
}
