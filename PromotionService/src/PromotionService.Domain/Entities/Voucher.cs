using PromotionService.Domain.Common;

namespace PromotionService.Domain.Entities
{
    public class Voucher : BaseEntity
    {
        public string VoucherCode { get; set; } = string.Empty;
        public Guid PromotionId { get; set; }
        public Promotion? Promotion { get; set; }
        public Guid? CustomerId { get; set; }
        public bool IsUsed { get; set; } = false;
        public DateTime? UsedAt { get; set; }
        public Guid? UsedInSaleId { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
