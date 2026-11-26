using System.Security.Claims;
using Guardou.Application.Authentication;
using Guardou.Domain.Authentication;
using Guardou.Infrastructure.Identity.Oidc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace Guardou.Infrastructure.Identity;

internal sealed class IdentitySessionService(
    SignInManager<ApplicationUser> signInManager,
    OidcSignOut oidcSignOut,
    IOptionsMonitor<BearerTokenOptions> bearerOptions,
    TimeProvider time)
    : ISessionService
{
    private const string IdTokenName = "id_token";

    private HttpContext Context => signInManager.Context;

    public async Task<PasswordLoginStatus> SignInAsync(string username, string password, SessionType type, bool persistent)
    {
        signInManager.AuthenticationScheme = type == SessionType.Token ? IdentityConstants.BearerScheme : IdentityConstants.ApplicationScheme;
        SignInResult result = await signInManager.PasswordSignInAsync(username, password, persistent, lockoutOnFailure: true);
        if (result.Succeeded)
        {
            return PasswordLoginStatus.SignedIn;
        }

        return result.IsLockedOut ? PasswordLoginStatus.LockedOut : PasswordLoginStatus.InvalidCredentials;
    }

    public async Task<bool> RefreshTokenAsync(string refreshToken)
    {
        AuthenticationTicket? ticket = bearerOptions.Get(IdentityConstants.BearerScheme).RefreshTokenProtector.Unprotect(refreshToken);
        if (ticket?.Properties.ExpiresUtc is not { } expires || time.GetUtcNow() >= expires)
        {
            return false;
        }

        ApplicationUser? user = await signInManager.ValidateSecurityStampAsync(ticket.Principal);
        if (user is null || await signInManager.UserManager.IsLockedOutAsync(user))
        {
            return false;
        }

        ClaimsPrincipal principal = await signInManager.CreateUserPrincipalAsync(user);
        await Context.SignInAsync(IdentityConstants.BearerScheme, principal);
        return true;
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
        if (SessionSchemes.UsesBearer(Context.Request))
        {
            return null;
        }

        string? idToken = await Context.GetTokenAsync(IdentityConstants.ApplicationScheme, IdTokenName);
        await signInManager.SignOutAsync();
        return idToken is null ? null : await oidcSignOut.EndSessionUrlAsync(Context.Request, idToken, cancellationToken);
    }

    public async Task RefreshAsync(string userId)
    {
        if (!SessionSchemes.UsesBearer(Context.Request) && await signInManager.UserManager.FindByIdAsync(userId) is { } user)
        {
            await signInManager.RefreshSignInAsync(user);
        }
    }
}
