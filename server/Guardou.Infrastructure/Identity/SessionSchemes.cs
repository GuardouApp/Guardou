using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace Guardou.Infrastructure.Identity;

public static class SessionSchemes
{
    private const string Default = "Guardou.Session";
    private const string BearerPrefix = "Bearer ";
    private static readonly TimeSpan AccessTokenLifetime = TimeSpan.FromHours(1);
    private static readonly TimeSpan RefreshTokenLifetime = TimeSpan.FromDays(14);

    public static bool UsesBearer(HttpRequest request)
    {
        return request.Headers.Authorization.ToString().StartsWith(BearerPrefix, StringComparison.OrdinalIgnoreCase);
    }

    internal static AuthenticationBuilder AddSessions(this AuthenticationBuilder authentication)
    {
        authentication.AddIdentityCookies();
        authentication.AddBearerToken(IdentityConstants.BearerScheme, ConfigureBearer);
        return authentication.AddPolicyScheme(Default, displayName: null, options =>
            options.ForwardDefaultSelector = context => UsesBearer(context.Request)
                ? IdentityConstants.BearerScheme
                : IdentityConstants.ApplicationScheme);
    }

    internal static void SelectDefault(AuthenticationOptions options)
    {
        options.DefaultScheme = Default;
    }

    private static void ConfigureBearer(BearerTokenOptions options)
    {
        options.BearerTokenExpiration = AccessTokenLifetime;
        options.RefreshTokenExpiration = RefreshTokenLifetime;
    }
}
