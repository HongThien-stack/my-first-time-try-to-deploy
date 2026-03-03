namespace InventoryService.Domain.Entities;

public class RestockRequestItem
{
    public Guid Id { get; set; }
    public Guid RequestId { get; set; }
    public Guid ProductId { get; set; } // ProductDB.products.id
    public int RequestedQuantity { get; set; }
    public int CurrentQuantity { get; set; } // Stock at time of request
    public int? ApprovedQuantity { get; set; }
    public string? Reason { get; set; }

    // Navigation properties
    public RestockRequest RestockRequest { get; set; } = null!;
}
