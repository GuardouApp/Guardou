using Guardou.Application.Settings;
using Guardou.Application.Users;
using Guardou.Domain.Authentication;
using Guardou.Domain.Users;

namespace Guardou.Application.Authentication;

public sealed class ExternalSignIn(
    EffectiveSettings<OidcSettings> settings,
    IUserAccountStore store,
    IProfilePictureStore pictures,
    ISessionService sessions)
{
    public async Task<bool> IsAvailableAsync(CancellationToken cancellationToken)
    {
        return (await settings.GetAsync(cancellationToken)).IsConfigured;
    }

    public async Task<ExternalSignInStatus> SignInAsync(CancellationToken cancellationToken)
    {
        OidcSettings oidc = await settings.GetAsync(cancellationToken);
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

        account ??= await CreateAsync(identity, cancellationToken);
        if (account is null)
        {
            return ExternalSignInStatus.Rejected;
        }

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

    private async Task<UserAccount?> CreateAsync(ExternalIdentity identity, CancellationToken cancellationToken)
    {
        if (identity.Email is null)
        {
            return null;
        }

        string username = await AvailableUsernameAsync(identity.UsernameBase);
        UserProfile profile = new(username, DisplayNameRule.Sanitize(identity.DisplayName, username), identity.Email, IsAdmin: false);
        UserAccountResult result = await store.CreateExternalAsync(profile, OidcSettings.Provider, identity.Key);
        if (!result.Succeeded || await store.FindByUsernameAsync(username) is not { } account)
        {
            return null;
        }

        if (identity.PictureUrl is { } picture)
        {
            await pictures.ImportAsync(account.Id, picture, cancellationToken);
        }

        return await store.FindAsync(account.Id);
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
