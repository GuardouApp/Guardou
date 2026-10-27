namespace Guardou.Application.Settings;

public sealed record EffectiveSetting(string Key, string? Value, bool Secret, SettingSource Source, string? Variable);
