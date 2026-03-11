namespace InventoryService.Application.DTOs;

/// <summary>
/// Structured error response for consistent error handling
/// </summary>
public class ErrorResponseDto
{
    public string Error { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public object? Details { get; set; }
}

/// <summary>
/// Common error codes
/// </summary>
public static class ErrorCodes
{
    // 400 Bad Request
    public const string InvalidInput = "InvalidInput";
    public const string NegativeQuantity = "NegativeQuantity";
    public const string ProductNotInInventory = "ProductNotInInventory";
    public const string InvalidCheckType = "InvalidCheckType";
    public const string InvalidLocationType = "InvalidLocationType";
    
    // 403 Forbidden
    public const string InsufficientPermissions = "InsufficientPermissions";
    public const string RoleNotAuthorized = "RoleNotAuthorized";
    public const string MissingClaim = "MissingClaim";
    
    // 404 Not Found
    public const string InventoryCheckNotFound = "InventoryCheckNotFound";
    public const string LocationNotFound = "LocationNotFound";
    public const string InventoryNotFound = "InventoryNotFound";
    public const string NoDiscrepancies = "NoDiscrepancies";
    
    // 409 Conflict
    public const string InvalidStateTransition = "InvalidStateTransition";
    public const string InventoryCheckAlreadySubmitted = "InventoryCheckAlreadySubmitted";
    public const string InventoryCheckAlreadyAdjusted = "InventoryCheckAlreadyAdjusted";
    public const string InventoryCheckNotApproved = "InventoryCheckNotApproved";
    public const string ActiveSessionExists = "ActiveSessionExists";
    public const string NotApproved = "NotApproved";
    
    // 500 Internal Server Error
    public const string InternalServerError = "InternalServerError";
    public const string TransactionFailed = "TransactionFailed";
}
