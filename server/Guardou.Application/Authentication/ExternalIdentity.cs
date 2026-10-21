using System.Security.Claims;
using Guardou.Domain.Authentication;
using Guardou.Domain.Users;

namespace Guardou.Application.Authentication;

public sealed record ExternalIdentity(
    string Key,
    string? Username,
    string? DisplayName,
    string? Email,
    bool EmailVerified,
    Uri? PictureUrl)
{
    private const string FallbackUsername = "user";

    public string UsernameBase =>
        new[] { Username, Email?.Split('@')[0] }
            .Select(value => UsernameRule.Sanitize(value ?? string.Empty))
            .FirstOrDefault(value => value.Length > 0)
        ?? FallbackUsername;

    public static ExternalIdentity? From(IReadOnlyList<Claim> claims, OidcSettings settings)
    {
        string? key = ValueOf(claims, OidcClaimTypes.Subject);
        return key is null
            ? null
            : new ExternalIdentity(
                key,
                ValueOf(claims, settings.UserClaim),
                ValueOf(claims, OidcClaimTypes.DisplayName),
                ValueOf(claims, OidcClaimTypes.Email),
                string.Equals(ValueOf(claims, OidcClaimTypes.EmailVerified), bool.TrueString, StringComparison.OrdinalIgnoreCase),
                PictureOf(ValueOf(claims, OidcClaimTypes.Picture)));
    }

    private static string? ValueOf(IReadOnlyList<Claim> claims, string type)
    {
        string? value = claims.FirstOrDefault(claim => claim.Type == type)?.Value.Trim();
        return string.IsNullOrEmpty(value) ? null : value;
    }

    private static Uri? PictureOf(string? value)
    {
        return Uri.TryCreate(value, UriKind.Absolute, out Uri? uri)
            && (uri.Scheme == Uri.UriSchemeHttps || uri.Scheme == Uri.UriSchemeHttp)
                ? uri
                : null;
    }
}
