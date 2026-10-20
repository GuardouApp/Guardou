namespace Guardou.Application.Settings;

internal static class SettingsUrl
{
    public static string? Normalize(string? value, string name)
    {
        string? trimmed = value?.Trim().TrimEnd('/');
        if (string.IsNullOrEmpty(trimmed))
        {
            return null;
        }

        return IsValid(trimmed)
            ? trimmed
            : throw new InvalidSettingsException($"The {name} must be an absolute HTTP or HTTPS URL without query and fragment.");
    }

    private static bool IsValid(string value)
    {
        return Uri.TryCreate(value, UriKind.Absolute, out Uri? uri)
            && (uri.Scheme == Uri.UriSchemeHttps || uri.Scheme == Uri.UriSchemeHttp)
            && uri.UserInfo.Length == 0
            && uri.Query.Length == 0
            && uri.Fragment.Length == 0;
    }
}
