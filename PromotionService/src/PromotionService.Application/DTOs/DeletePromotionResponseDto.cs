namespace PromotionService.Application.DTOs
{
    public class DeletePromotionResponseDto
    {
        public Guid Id { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsActive { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
