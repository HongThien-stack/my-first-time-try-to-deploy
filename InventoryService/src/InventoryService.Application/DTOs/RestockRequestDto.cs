namespace InventoryService.Application.DTOs;

public class RestockRequestDto
{
    public Guid Id { get; set; }
    public string RequestNumber { get; set; } = string.Empty;
    public Guid StoreId { get; set; }
    public Guid? WarehouseId { get; set; }
    public Guid RequestedBy { get; set; }
    public DateTime RequestedDate { get; set; }
    public string Priority { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public Guid? ApprovedBy { get; set; }
    public DateTime? ApprovedDate { get; set; }
    public Guid? TransferId { get; set; }
    public string? Notes { get; set; }
    public List<RestockRequestItemDto> Items { get; set; } = new();
}

public class RestockRequestItemDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public int RequestedQuantity { get; set; }
    public int CurrentQuantity { get; set; }
    public int? ApprovedQuantity { get; set; }
    public string? Reason { get; set; }
}

public class CreateRestockRequestDto
{
    public Guid StoreId { get; set; }
    public Guid RequestedBy { get; set; }
    public string Priority { get; set; } = "NORMAL";
    public string? Notes { get; set; }
    public List<CreateRestockRequestItemDto> Items { get; set; } = new();
}

public class CreateRestockRequestItemDto
{
    public Guid ProductId { get; set; }
    public int RequestedQuantity { get; set; }
    public int CurrentQuantity { get; set; }
    public string? Reason { get; set; }
}
