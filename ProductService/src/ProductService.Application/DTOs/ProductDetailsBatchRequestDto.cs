namespace ProductService.Application.DTOs
{
    public class ProductDetailsBatchRequestDto
    {
        public List<Guid> ProductIds { get; set; } = new List<Guid>();
    }
}
