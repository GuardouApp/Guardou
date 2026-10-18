using System.Security.Claims;

namespace Guardou.Api.Authentication;

internal static class ClaimsPrincipalExtensions
{
    public static string UserId(this ClaimsPrincipal principal)
    {
        return principal.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new InvalidOperationException("The signed in user has no id.");
    }
}
