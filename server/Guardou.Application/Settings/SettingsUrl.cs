namespace Guardou.Application.Settings;

internal static class SettingsUrl
{
    private static readonly string[] HttpSchemes = [Uri.UriSchemeHttps, Uri.UriSchemeHttp];
    private static readonly string[] HttpsSchemes = [Uri.UriSchemeHttps];

    public static string? Normalize(string? value, string name)
    {
        return Normalize(value, name, HttpSchemes, "HTTP or HTTPS");
    }

    public static string? NormalizeHttps(string? value, string name)
    {
        return Normalize(value, name, HttpsSchemes, "HTTPS");
    }

    private static string? Normalize(string? value, string name, string[] schemes, string schemesText)
    {
        string? trimmed = value?.Trim().TrimEnd('/');
        if (string.IsNullOrEmpty(trimmed))
        {
            return null;
        }

        return IsValid(trimmed, schemes)
            ? trimmed
            : throw new InvalidSettingsException($"The {name} must be an absolute {schemesText} URL without query and fragment.");
    }

    private static bool IsValid(string value, string[] schemes)
    {
        return Uri.TryCreate(value, UriKind.Absolute, out Uri? uri)
            && schemes.Contains(uri.Scheme)
            && uri.UserInfo.Length == 0
            && uri.Query.Length == 0
            && uri.Fragment.Length == 0;
    }
}
