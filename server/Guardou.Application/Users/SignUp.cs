using Guardou.Application.Settings;
using Guardou.Domain.Authentication;
using Guardou.Domain.Settings;

namespace Guardou.Application.Users;

public sealed class SignUp(
    IUserAccountStore store,
    EffectiveSettings<ServerSettings> server,
    EffectiveSettings<OidcSettings> oidc)
{
    public async Task<bool> IsOpenAsync(CancellationToken cancellationToken)
    {
        return (await server.GetAsync(cancellationToken)).Signup
            && (await oidc.GetAsync(cancellationToken)).AllowsPasswordLogin
            && await store.AnyUserAsync();
    }

    public async Task<RegistrationResult> RegisterAsync(UserProfile profile, string password, CancellationToken cancellationToken)
    {
        if (!await IsOpenAsync(cancellationToken))
        {
            return RegistrationResult.Closed;
        }

        UserAccountResult result = await store.CreateAsync(profile with { IsAdmin = false }, password);
        return result.Succeeded
            ? RegistrationResult.Registered
            : RegistrationResult.Rejected(result.Errors.Select(error => error.Description).ToList());
    }
}
