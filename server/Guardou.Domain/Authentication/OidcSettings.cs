using Guardou.Domain.Settings;

namespace Guardou.Domain.Authentication;

public sealed record OidcSettings(
    string? Issuer,
    string? ClientId,
    string? ClientSecret,
    IReadOnlyList<string> Scopes,
    string UserClaim,
    bool AutoCreateUsers,
    bool PasswordLogin) : ISettings<OidcSettings>
{
    public const string Provider = "oidc";
    public const string OpenIdScope = "openid";

    public static string Key => Provider;

    public static OidcSettings Initial { get; } = new(
        Issuer: null,
        ClientId: null,
        ClientSecret: null,
        Scopes: [OpenIdScope, "profile", "email"],
        UserClaim: "preferred_username",
        AutoCreateUsers: true,
        PasswordLogin: true);

    public bool IsConfigured => Issuer is not null && ClientId is not null;

    public bool AllowsPasswordLogin => PasswordLogin || !IsConfigured;
}
