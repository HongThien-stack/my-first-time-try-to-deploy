// These DTOs will represent the data received from other microservices.
// They are internal to the PosService Application layer.

namespace PosService.Application.DTOs.External
{
    // From ProductService
    public class ProductDetailsDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Sku { get; set; } = string.Empty;
        public string? Barcode { get; set; }
        public decimal Price { get; set; }
        public string? ImageUrl { get; set; }
        public string? Brand { get; set; }
        public string Unit { get; set; } = string.Empty;
        public bool IsOnSale { get; set; }
        public Guid CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
    }

    // From PromotionService
    public class PromotionCalculationRequestDto
    {
        public Guid? CustomerId { get; set; }
        public string? VoucherCode { get; set; }
        public List<PromotionCartItemDto> Items { get; set; } = new();
    }

    public class PromotionCartItemDto
    {
        public Guid ProductId { get; set; }
        public Guid CategoryId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }

    public class PromotionCalculationResultDto
    {
        public decimal Subtotal { get; set; }
        public decimal TotalDiscountAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public int PointsEarned { get; set; }
        public List<AppliedPromotionDto> AppliedPromotions { get; set; } = new();
    }

    public class AppliedPromotionDto
    {
        public Guid PromotionId { get; set; }
        public string PromotionName { get; set; } = string.Empty;
        public decimal DiscountAmount { get; set; }
    }
}
