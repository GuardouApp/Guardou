using Guardou.Application.Settings;
using Guardou.Application.Users;
using Guardou.Domain.Authentication;

namespace Guardou.Application.Authentication;

public sealed class ExternalSignIn(
    ISettingsStore settings,
    IUserAccountStore store,
    ISessionService sessions)
{
    public async Task<bool> IsAvailableAsync(CancellationToken cancellationToken)
    {
        return (await settings.GetAsync<OidcSettings>(cancellationToken)).IsConfigured;
    }

    public async Task<ExternalSignInStatus> SignInAsync(CancellationToken cancellationToken)
    {
        OidcSettings oidc = await settings.GetAsync<OidcSettings>(cancellationToken);
        ExternalLogin? login = await sessions.ReadExternalLoginAsync();
        ExternalIdentity? identity = login is null ? null : ExternalIdentity.From(login.Claims, oidc);
        if (!oidc.IsConfigured || login is null || identity is null)
        {
            return ExternalSignInStatus.Failed;
        }

        UserAccount? account = await FindAsync(identity);
        if (account is null && !oidc.AutoCreateUsers)
        {
            return ExternalSignInStatus.NoAccount;
        }

        account ??= await CreateAsync(identity);
        if (account is null)
        {
            return ExternalSignInStatus.Rejected;
        }

        await store.SetPictureAsync(account.Id, identity.PictureUrl);
        await sessions.SignInExternalAsync(account.Id, login);
        return ExternalSignInStatus.SignedIn;
    }

    private async Task<UserAccount?> FindAsync(ExternalIdentity identity)
    {
        if (await store.FindByLoginAsync(OidcSettings.Provider, identity.Key) is { } linked)
        {
            return linked;
        }

        if (!identity.EmailVerified
            || identity.Email is null
            || await store.FindByEmailAsync(identity.Email) is not { } account
            || account.ExternalLogins.Contains(OidcSettings.Provider))
        {
            return null;
        }

        UserAccountResult result = await store.AddLoginAsync(account.Id, OidcSettings.Provider, identity.Key);
        return result.Succeeded ? account : null;
    }

    private async Task<UserAccount?> CreateAsync(ExternalIdentity identity)
    {
        if (identity.Email is null)
        {
            return null;
        }

        string username = await AvailableUsernameAsync(identity.UsernameBase);
        UserProfile profile = new(username, identity.Email, IsAdmin: false);
        UserAccountResult result = await store.CreateExternalAsync(profile, OidcSettings.Provider, identity.Key);
        return result.Succeeded ? await store.FindByUsernameAsync(username) : null;
    }

    private async Task<string> AvailableUsernameAsync(string username)
    {
        string candidate = username;
        for (int number = 2; await store.FindByUsernameAsync(candidate) is not null; number++)
        {
            candidate = $"{username}{number}";
        }

        return candidate;
    }
}
