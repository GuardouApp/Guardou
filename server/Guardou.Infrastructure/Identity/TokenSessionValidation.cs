using System.Security.Claims;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace Guardou.Infrastructure.Identity;

public static class TokenSessionValidation
{
    public static IApplicationBuilder UseTokenSessionValidation(this IApplicationBuilder app)
    {
        return app.Use(ValidateAsync);
    }

    private static async Task ValidateAsync(HttpContext context, RequestDelegate next)
    {
        if (SessionSchemes.UsesBearer(context.Request) && context.User.Identity?.IsAuthenticated == true)
        {
            SignInManager<ApplicationUser> signInManager = context.RequestServices.GetRequiredService<SignInManager<ApplicationUser>>();
            ApplicationUser? user = await signInManager.ValidateSecurityStampAsync(context.User);
            context.User = user is null ? new ClaimsPrincipal(new ClaimsIdentity()) : await signInManager.CreateUserPrincipalAsync(user);
        }

        await next(context);
    }
}
