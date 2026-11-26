using Guardou.Application.Settings;
using Guardou.Domain.Authentication;

namespace Guardou.Application.Authentication;

public sealed class PasswordLogin(EffectiveSettings<OidcSettings> settings, ISessionService sessions)
{
    public async Task<PasswordLoginStatus> SignInAsync(
        string username,
        string password,
        SessionType type,
        bool persistent,
        CancellationToken cancellationToken)
    {
        OidcSettings oidc = await settings.GetAsync(cancellationToken);
        return oidc.AllowsPasswordLogin
            ? await sessions.SignInAsync(username, password, type, persistent)
            : PasswordLoginStatus.Disabled;
    }
}
