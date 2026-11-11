using Guardou.Domain.Authentication;
using Microsoft.AspNetCore.DataProtection;

namespace Guardou.Infrastructure.Persistence;

internal sealed class SettingsSecrets(IDataProtectionProvider provider)
{
    private const string Prefix = "protected:";
    private const string Purpose = "Guardou.Settings.OidcClientSecret";

    private readonly IDataProtector _protector = provider.CreateProtector(Purpose);

    public TSettings Protect<TSettings>(TSettings settings)
    {
        return settings is OidcSettings { ClientSecret: { } secret } oidc && !IsProtected(secret)
            ? (TSettings)(object)(oidc with { ClientSecret = Prefix + _protector.Protect(secret) })
            : settings;
    }

    public TSettings Unprotect<TSettings>(TSettings settings)
    {
        return settings is OidcSettings { ClientSecret: { } secret } oidc && IsProtected(secret)
            ? (TSettings)(object)(oidc with { ClientSecret = _protector.Unprotect(secret[Prefix.Length..]) })
            : settings;
    }

    public static bool IsProtected(string secret)
    {
        return secret.StartsWith(Prefix, StringComparison.Ordinal);
    }
}
