namespace Guardou.Domain.Users;

public static class DisplayNameRule
{
    public const int MaximumLength = 100;

    public static bool IsValid(string displayName)
    {
        return displayName.Length is > 0 and <= MaximumLength && displayName == displayName.Trim();
    }

    public static string Normalize(string displayName)
    {
        return displayName.Trim();
    }

    public static string Sanitize(string? value, string fallback)
    {
        string displayName = Normalize(value ?? string.Empty);
        if (displayName.Length == 0)
        {
            return fallback;
        }

        if (displayName.Length <= MaximumLength)
        {
            return displayName;
        }

        int length = char.IsHighSurrogate(displayName[MaximumLength - 1]) ? MaximumLength - 1 : MaximumLength;
        return displayName[..length].TrimEnd();
    }
}
