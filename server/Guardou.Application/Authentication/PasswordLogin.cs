using Guardou.Application.Settings;
using Guardou.Domain.Authentication;

namespace Guardou.Application.Authentication;

public sealed class PasswordLogin(ISettingsStore settings, ISessionService sessions)
{
    public async Task<PasswordLoginStatus> SignInAsync(
        string username,
        string password,
        bool persistent,
        CancellationToken cancellationToken)
    {
        OidcSettings oidc = await settings.GetAsync<OidcSettings>(cancellationToken);
        if (!oidc.AllowsPasswordLogin)
        {
            return PasswordLoginStatus.Disabled;
        }

        return await sessions.SignInAsync(username, password, persistent)
            ? PasswordLoginStatus.SignedIn
            : PasswordLoginStatus.InvalidCredentials;
    }
}
