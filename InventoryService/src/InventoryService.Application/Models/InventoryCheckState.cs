namespace InventoryService.Application.Models;

/// <summary>
/// Internal state tracking for inventory check workflow
/// Since schema doesn't have dedicated columns for approval/adjustment,
/// we parse and maintain this state from the notes field.
/// </summary>
public class InventoryCheckState
{
    public bool IsApproved { get; set; }
    public Guid? ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }
    
    public bool IsAdjusted { get; set; }
    public Guid? AdjustedBy { get; set; }
    public DateTime? AdjustedAt { get; set; }
    
    /// <summary>
    /// Parse state from notes field
    /// </summary>
    public static InventoryCheckState ParseFromNotes(string? notes)
    {
        var state = new InventoryCheckState();
        
        if (string.IsNullOrEmpty(notes))
        {
            return state;
        }
        
        // Parse approval state
        if (notes.Contains("[APPROVED"))
        {
            state.IsApproved = true;
            // Extract user ID and timestamp if needed
            var approvedMatch = System.Text.RegularExpressions.Regex.Match(
                notes, 
                @"\[APPROVED by ([a-f0-9-]+) at ([^\]]+)\]"
            );
            if (approvedMatch.Success)
            {
                if (Guid.TryParse(approvedMatch.Groups[1].Value, out var approvedBy))
                {
                    state.ApprovedBy = approvedBy;
                }
                if (DateTime.TryParse(approvedMatch.Groups[2].Value, out var approvedAt))
                {
                    state.ApprovedAt = approvedAt;
                }
            }
        }
        
        // Parse adjustment state
        if (notes.Contains("[ADJUSTED"))
        {
            state.IsAdjusted = true;
            var adjustedMatch = System.Text.RegularExpressions.Regex.Match(
                notes,
                @"\[ADJUSTED by ([a-f0-9-]+) at ([^\]]+)\]"
            );
            if (adjustedMatch.Success)
            {
                if (Guid.TryParse(adjustedMatch.Groups[1].Value, out var adjustedBy))
                {
                    state.AdjustedBy = adjustedBy;
                }
                if (DateTime.TryParse(adjustedMatch.Groups[2].Value, out var adjustedAt))
                {
                    state.AdjustedAt = adjustedAt;
                }
            }
        }
        
        return state;
    }
}

/// <summary>
/// User context from authentication
/// </summary>
public class UserContext
{
    public Guid UserId { get; set; }
    public string Role { get; set; } = string.Empty; // STAFF | MANAGER
    
    public bool IsManager => Role.Equals("Manager", StringComparison.OrdinalIgnoreCase)
        || Role.Equals("Store Manager", StringComparison.OrdinalIgnoreCase)
        || Role.Equals("Warehouse Manager", StringComparison.OrdinalIgnoreCase)
        || Role.Equals("Admin", StringComparison.OrdinalIgnoreCase);
    
    public bool IsStaff => Role.Equals("Store Staff", StringComparison.OrdinalIgnoreCase)
        || Role.Equals("Warehouse Staff", StringComparison.OrdinalIgnoreCase)
        || IsManager;
}
