namespace PosService.Application.DTOs;

public class AddCartItemDto
{
    public string Barcode { get; set; } = string.Empty;
    public int Quantity { get; set; } = 1;
}
