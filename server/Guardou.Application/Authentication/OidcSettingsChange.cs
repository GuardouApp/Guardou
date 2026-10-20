using System.ComponentModel.DataAnnotations;

namespace Guardou.Application.Authentication;

public sealed record OidcSettingsChange(
    string? Issuer,
    string? ClientId,
    string? ClientSecret,
    [Required] IReadOnlyList<string> Scopes,
    [Required] string UserClaim,
    bool AutoCreateUsers,
    bool PasswordLogin);
