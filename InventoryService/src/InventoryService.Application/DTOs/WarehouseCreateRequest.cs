namespace InventoryService.Application.DTOs;

public class WarehouseCreateRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Location { get; set; }
    public int Capacity { get; set; }
    public string Status { get; set; } = "ACTIVE";
    public Guid? ParentId { get; set; }
    public Guid? CreatedBy { get; set; }
}
