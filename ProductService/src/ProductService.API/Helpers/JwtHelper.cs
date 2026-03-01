using System.Security.Claims;

namespace ProductService.API.Helpers;

public static class JwtHelper
{
    /// <summary>
    /// Lấy User ID từ JWT Claims
    /// </summary>
    public static Guid? GetUserId(ClaimsPrincipal user)
    {
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier) 
                       ?? user.FindFirst("sub") 
                       ?? user.FindFirst("user_id");
        
        if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId))
        {
            return userId;
        }
        
        return null;
    }

    /// <summary>
    /// Lấy User Email từ JWT Claims
    /// </summary>
    public static string? GetUserEmail(ClaimsPrincipal user)
    {
        return user.FindFirst(ClaimTypes.Email)?.Value 
            ?? user.FindFirst("email")?.Value;
    }

    /// <summary>
    /// Lấy User Full Name từ JWT Claims
    /// </summary>
    public static string? GetUserFullName(ClaimsPrincipal user)
    {
        return user.FindFirst(ClaimTypes.Name)?.Value 
            ?? user.FindFirst("full_name")?.Value 
            ?? user.FindFirst("name")?.Value;
    }

    /// <summary>
    /// Lấy User Role từ JWT Claims
    /// </summary>
    public static string? GetUserRole(ClaimsPrincipal user)
    {
        return user.FindFirst(ClaimTypes.Role)?.Value 
            ?? user.FindFirst("role")?.Value;
    }

    /// <summary>
    /// Kiểm tra user có role cụ thể không
    /// </summary>
    public static bool HasRole(ClaimsPrincipal user, params string[] roles)
    {
        var userRole = GetUserRole(user);
        return userRole != null && roles.Contains(userRole, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Kiểm tra user có quyền tạo sản phẩm không
    /// Chỉ Admin, Manager, Warehouse Staff mới có quyền
    /// </summary>
    public static bool CanCreateProduct(ClaimsPrincipal user)
    {
        return HasRole(user, "Admin", "Manager", "Warehouse Staff");
    }

    /// <summary>
    /// Lấy IP Address từ HttpContext
    /// </summary>
    public static string? GetIpAddress(Microsoft.AspNetCore.Http.HttpContext context)
    {
        return context.Request.Headers["X-Forwarded-For"].FirstOrDefault()
            ?? context.Connection.RemoteIpAddress?.ToString();
    }
}
