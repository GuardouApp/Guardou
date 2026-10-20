namespace Guardou.Application.Authentication;

public static class OidcEndpoints
{
    public const string CallbackPath = "/api/auth/oidc/callback";
    public const string LoginPath = "/login";
    public const string ErrorParameter = "oidcError";

    public static string Callback(string origin)
    {
        return origin + CallbackPath;
    }

    public static string SignedOut(string origin)
    {
        return origin + LoginPath;
    }

    public static string LoginError(string code)
    {
        return $"{LoginPath}?{ErrorParameter}={Uri.EscapeDataString(code)}";
    }
}
