using Guardou.Application.Settings;
using Guardou.Application.Users;
using Guardou.Domain;
using Guardou.Domain.Authentication;
using Guardou.Domain.Users;

namespace Guardou.Application.Configuration;

public sealed class RuntimeConfigurationQuery(Registration registration, ISettingsStore settings)
{
    public async Task<RuntimeConfiguration> GetAsync(CancellationToken cancellationToken)
    {
        OidcSettings oidc = await settings.GetAsync<OidcSettings>(cancellationToken);
        return new RuntimeConfiguration(
            Product.Name,
            MethodsOf(oidc),
            await registration.IsOpenAsync(),
            PasswordPolicy.Default);
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
