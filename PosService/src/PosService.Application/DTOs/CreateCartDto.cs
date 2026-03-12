namespace PosService.Application.DTOs;

public class CreateCartDto
{
    public Guid StoreId { get; set; }
    public Guid CashierId { get; set; }
    public Guid? CustomerId { get; set; }
    public string? Notes { get; set; }
}
