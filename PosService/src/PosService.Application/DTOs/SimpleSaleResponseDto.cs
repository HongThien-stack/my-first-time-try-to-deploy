namespace PosService.Application.DTOs
{
    /// <summary>
    /// Simple Sale Response
    /// </summary>
    public class SimpleSaleResponseDto
    {
        public Guid SaleId { get; set; }
        public string SaleNumber { get; set; } = string.Empty;
        public decimal Subtotal { get; set; }
        public decimal TotalAmount { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty; // COMPLETED or PENDING
        public string PaymentStatus { get; set; } = string.Empty; // PAID or PENDING
        public DateTime SaleDate { get; set; }
        
        // For MOMO payment
        public string? MomoPayUrl { get; set; }
        public string? MomoQrUrl { get; set; }
        public Guid? PaymentId { get; set; }
        
        public List<SimpleSaleItemResponseDto> Items { get; set; } = new();
    }

    public class SimpleSaleItemResponseDto
    {
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal LineTotal { get; set; }
    }
}
