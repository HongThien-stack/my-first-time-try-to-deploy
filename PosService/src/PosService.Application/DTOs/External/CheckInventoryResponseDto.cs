namespace PosService.Application.DTOs.External;

public class CheckInventoryResponseDto
{
    public bool IsAvailable { get; set; }
    public List<UnavailableItemDto> UnavailableItems { get; set; } = new();
}

public class UnavailableItemDto
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int RequestedQty { get; set; }
    public int AvailableQty { get; set; }
}
