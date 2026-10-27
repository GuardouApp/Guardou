using System.Text.Json;

namespace Guardou.Application.Settings;

internal sealed record SettingField<TSettings>(
    string Property,
    Func<TSettings, string?> Text,
    SettingVariable<TSettings>? Variable = null,
    bool Secret = false)
{
    public string Name => JsonNamingPolicy.CamelCase.ConvertName(Property);
}
