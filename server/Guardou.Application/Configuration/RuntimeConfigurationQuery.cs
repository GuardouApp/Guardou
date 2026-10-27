using Guardou.Application.Settings;
using Guardou.Application.Users;
using Guardou.Domain;
using Guardou.Domain.Authentication;
using Guardou.Domain.Settings;
using Guardou.Domain.Users;

namespace Guardou.Application.Configuration;

public sealed class RuntimeConfigurationQuery(
    Registration registration,
    EffectiveSettings<OidcSettings> oidcSettings,
    EffectiveSettings<ServerSettings> server)
{
    public async Task<RuntimeConfiguration> GetAsync(CancellationToken cancellationToken)
    {
        OidcSettings oidc = await oidcSettings.GetAsync(cancellationToken);
        return new RuntimeConfiguration(
            Product.Name,
            MethodsOf(oidc),
            await registration.IsOpenAsync(),
            PasswordPolicy.Default,
            (await server.GetAsync(cancellationToken)).TrashRetentionDays);
    }

    private static List<AuthenticationMethod> MethodsOf(OidcSettings oidc)
    {
        List<AuthenticationMethod> methods = [];
        if (oidc.AllowsPasswordLogin)
        {
            methods.Add(AuthenticationMethod.Password);
        }

        if (oidc.IsConfigured)
        {
            methods.Add(AuthenticationMethod.Oidc);
        }

        return methods;
    }
}
