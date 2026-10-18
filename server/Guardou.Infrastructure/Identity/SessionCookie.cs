using Guardou.Domain;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;

namespace Guardou.Infrastructure.Identity;

internal static class SessionCookie
{
    private static readonly TimeSpan Lifetime = TimeSpan.FromDays(7);

    public static void Configure(CookieAuthenticationOptions options)
    {
        options.Cookie.Name = Product.CookieName("session");
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Strict;
        options.ExpireTimeSpan = Lifetime;
        options.SlidingExpiration = true;
        options.Events.OnRedirectToLogin = context => WriteStatusCode(context, StatusCodes.Status401Unauthorized);
        options.Events.OnRedirectToAccessDenied = context => WriteStatusCode(context, StatusCodes.Status403Forbidden);
    }

    private static Task WriteStatusCode(RedirectContext<CookieAuthenticationOptions> context, int statusCode)
    {
        context.Response.StatusCode = statusCode;
        return Task.CompletedTask;
    }
}
