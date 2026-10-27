using Guardou.Application.Settings;
using Guardou.Domain.Authentication;

namespace Guardou.Application.Authentication;

internal sealed class OidcSettingsSchema : ISettingsSchema<OidcSettings>
{
    public IReadOnlyList<SettingField<OidcSettings>> Fields { get; } =
    [
        new(
            nameof(OidcSettings.Issuer),
            settings => settings.Issuer,
            new(SettingsVariables.OidcIssuer, overrides => overrides.OidcIssuer is not null, (target, source) => target with { Issuer = source.Issuer })),
        new(
            nameof(OidcSettings.ClientId),
            settings => settings.ClientId,
            new(SettingsVariables.OidcClientId, overrides => overrides.OidcClientId is not null, (target, source) => target with { ClientId = source.ClientId })),
        new(
            nameof(OidcSettings.ClientSecret),
            settings => settings.ClientSecret,
            new(SettingsVariables.OidcClientSecret, overrides => overrides.OidcClientSecret is not null, (target, source) => target with { ClientSecret = source.ClientSecret }),
            Secret: true),
        new(
            nameof(OidcSettings.Scopes),
            settings => string.Join(' ', settings.Scopes),
            new(SettingsVariables.OidcScopes, overrides => overrides.OidcScopes is not null, (target, source) => target with { Scopes = source.Scopes })),
        new(
            nameof(OidcSettings.UserClaim),
            settings => settings.UserClaim,
            new(SettingsVariables.OidcUserClaim, overrides => overrides.OidcUserClaim is not null, (target, source) => target with { UserClaim = source.UserClaim })),
        new(nameof(OidcSettings.AutoCreateUsers), settings => Text(settings.AutoCreateUsers)),
        new(nameof(OidcSettings.PasswordLogin), settings => Text(settings.PasswordLogin)),
    ];

    public static string? Issuer(string? value)
    {
        return SettingsUrl.NormalizeHttps(value, "issuer");
    }

    public static List<string> Scopes(IEnumerable<string> scopes)
    {
        return scopes
            .SelectMany(scope => scope.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries))
            .Prepend(OidcSettings.OpenIdScope)
            .Distinct(StringComparer.Ordinal)
            .ToList();
    }

    public OidcSettings FromEnvironment(SettingsOverrides overrides)
    {
        OidcSettings initial = OidcSettings.Initial;
        return initial with
        {
            Issuer = Issuer(overrides.OidcIssuer),
            ClientId = SettingsText.Optional(overrides.OidcClientId),
            ClientSecret = SettingsText.Optional(overrides.OidcClientSecret),
            Scopes = overrides.OidcScopes is { } scopes ? Scopes([scopes]) : initial.Scopes,
            UserClaim = overrides.OidcUserClaim is { } claim ? SettingsText.Required(claim, "user claim") : initial.UserClaim,
        };
    }

    private static string Text(bool value)
    {
        return value ? "true" : "false";
    }
}
