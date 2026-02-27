using IdentityService.Domain.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace IdentityService.API.Filters;

/// <summary>
/// Authorization filter that blocks access for users whose email is not yet verified.
/// Apply this attribute on controllers or actions that require a verified email.
/// Usage: [RequireVerifiedEmail]
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public class RequireVerifiedEmailAttribute : Attribute, IAsyncAuthorizationFilter
{
    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var user = context.HttpContext.User;

        // Must be authenticated first
        if (user?.Identity?.IsAuthenticated != true)
        {
            context.Result = new UnauthorizedObjectResult(new
            {
                success = false,
                message = "Authentication required."
            });
            return;
        }

        var userIdClaim = user.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            context.Result = new UnauthorizedObjectResult(new
            {
                success = false,
                message = "Invalid user identity."
            });
            return;
        }

        // Resolve repository from DI
        var userRepository = context.HttpContext.RequestServices.GetRequiredService<IUserRepository>();
        var dbUser = await userRepository.GetByIdAsync(userId);

        if (dbUser == null)
        {
            context.Result = new UnauthorizedObjectResult(new
            {
                success = false,
                message = "User not found."
            });
            return;
        }

        if (!dbUser.EmailVerified)
        {
            context.Result = new ObjectResult(new
            {
                success = false,
                message = "Email verification required. Please verify your email before accessing this resource.",
                isEmailVerified = false
            })
            {
                StatusCode = StatusCodes.Status403Forbidden
            };
        }
    }
}
