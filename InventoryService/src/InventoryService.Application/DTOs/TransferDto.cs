namespace InventoryService.Application.DTOs;

public class TransferDto
{
    public Guid Id { get; set; }
    public string TransferNumber { get; set; } = string.Empty;
    public string FromLocationType { get; set; } = string.Empty;
    public Guid FromLocationId { get; set; }
    public string ToLocationType { get; set; } = string.Empty;
    public Guid ToLocationId { get; set; }
    public DateTime TransferDate { get; set; }
    public DateTime? ExpectedDelivery { get; set; }
    public DateTime? ActualDelivery { get; set; }
    public string Status { get; set; } = string.Empty;
    public Guid? ShippedBy { get; set; }
    public Guid? ReceivedBy { get; set; }
    public string? Notes { get; set; }
    public List<TransferItemDto> Items { get; set; } = new();
}

public class TransferItemDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public Guid? BatchId { get; set; }
    public int RequestedQuantity { get; set; }
    public int? ShippedQuantity { get; set; }
    public int? ReceivedQuantity { get; set; }
    public int DamagedQuantity { get; set; }
    public string? Notes { get; set; }
}

public class CreateTransferDto
{
    public string FromLocationType { get; set; } = string.Empty;
    public Guid FromLocationId { get; set; }
    public string ToLocationType { get; set; } = string.Empty;
    public Guid ToLocationId { get; set; }
    public DateTime? ExpectedDelivery { get; set; }
    public Guid ShippedBy { get; set; }
    public string? Notes { get; set; }
    public List<CreateTransferItemDto> Items { get; set; } = new();
}

public class CreateTransferItemDto
{
    public Guid ProductId { get; set; }
    public Guid? BatchId { get; set; }
    public int RequestedQuantity { get; set; }
}
