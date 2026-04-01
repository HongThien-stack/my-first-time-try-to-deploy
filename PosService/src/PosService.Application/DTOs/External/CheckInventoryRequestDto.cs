namespace PosService.Application.DTOs.External;

public class CheckInventoryRequestDto
{
    public Guid StoreId { get; set; }
    public List<CheckInventoryItemDto> Items { get; set; } = new();
}

public class CheckInventoryItemDto
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
}
