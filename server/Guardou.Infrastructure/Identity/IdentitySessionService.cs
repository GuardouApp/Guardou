using Guardou.Application.Authentication;
using Guardou.Domain.Authentication;
using Guardou.Infrastructure.Identity.Oidc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

namespace Guardou.Infrastructure.Identity;

internal sealed class IdentitySessionService(SignInManager<ApplicationUser> signInManager, OidcSignOut oidcSignOut) : ISessionService
{
    private const string IdTokenName = "id_token";

    private HttpContext Context => signInManager.Context;

    public async Task<bool> SignInAsync(string username, string password, bool persistent)
    {
        SignInResult result = await signInManager.PasswordSignInAsync(username, password, persistent, lockoutOnFailure: false);
        return result.Succeeded;
    }

    public async Task<ExternalLogin?> ReadExternalLoginAsync()
    {
        AuthenticateResult result = await Context.AuthenticateAsync(IdentityConstants.ExternalScheme);
        return result.Succeeded
            ? new ExternalLogin(result.Principal.Claims.ToList(), result.Properties.GetTokenValue(IdTokenName))
            : null;
    }

    public async Task SignInExternalAsync(string userId, ExternalLogin login)
    {
        ApplicationUser user = await signInManager.UserManager.FindByIdAsync(userId)
            ?? throw new InvalidOperationException($"The user '{userId}' does not exist.");
        AuthenticationProperties properties = new() { IsPersistent = true };
        if (login.IdToken is { } idToken)
        {
            properties.StoreTokens([new AuthenticationToken { Name = IdTokenName, Value = idToken }]);
        }

        await Context.SignOutAsync(IdentityConstants.ExternalScheme);
        await signInManager.SignInAsync(user, properties, OidcSettings.Provider);
    }

    public async Task<string?> SignOutAsync(CancellationToken cancellationToken)
    {
        string? idToken = await Context.GetTokenAsync(IdentityConstants.ApplicationScheme, IdTokenName);
        await signInManager.SignOutAsync();
        return idToken is null ? null : await oidcSignOut.EndSessionUrlAsync(Context.Request, idToken, cancellationToken);
    }

    public async Task RefreshAsync(string userId)
    {
        if (await signInManager.UserManager.FindByIdAsync(userId) is { } user)
        {
            await signInManager.RefreshSignInAsync(user);
        }
    }
}
