namespace Guardou.Application.Settings;

internal interface ISettingsSchema<TSettings>
{
    IReadOnlyList<SettingField<TSettings>> Fields { get; }

    TSettings FromEnvironment(SettingsOverrides overrides);
}
