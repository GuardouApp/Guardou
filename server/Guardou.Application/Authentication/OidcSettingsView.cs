namespace Guardou.Application.Authentication;

public sealed record OidcSettingsView(
    string? Issuer,
    string? ClientId,
    bool ClientSecretSet,
    IReadOnlyList<string> Scopes,
    string UserClaim,
    bool AutoCreateUsers,
    bool PasswordLogin,
    string RedirectUri,
    string PostLogoutRedirectUri);
