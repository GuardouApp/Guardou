namespace Guardou.Application.Settings;

internal sealed record SettingVariable<TSettings>(
    string Name,
    Func<SettingsOverrides, bool> IsSet,
    Func<TSettings, TSettings, TSettings> Copy);
