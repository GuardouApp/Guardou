namespace Guardou.Application.Settings;

internal static class SettingsText
{
    public static string? Optional(string? value)
    {
        string? trimmed = value?.Trim();
        return string.IsNullOrEmpty(trimmed) ? null : trimmed;
    }

    public static string Required(string value, string name)
    {
        return Optional(value) ?? throw new InvalidSettingsException($"The {name} is required.");
    }
}
