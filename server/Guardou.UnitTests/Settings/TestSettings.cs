using Guardou.Application.Authentication;
using Guardou.Application.Settings;
using Guardou.Domain.Authentication;
using Guardou.Domain.Settings;
using Microsoft.Extensions.Options;

namespace Guardou.UnitTests.Settings;

internal static class TestSettings
{
    public static EffectiveSettings<ServerSettings> Server(ISettingsStore store, SettingsOverrides? overrides = null)
    {
        return new EffectiveSettings<ServerSettings>(store, new ServerSettingsSchema(), Options.Create(overrides ?? new SettingsOverrides()));
    }

    public static EffectiveSettings<OidcSettings> Oidc(ISettingsStore store, SettingsOverrides? overrides = null)
    {
        return new EffectiveSettings<OidcSettings>(store, new OidcSettingsSchema(), Options.Create(overrides ?? new SettingsOverrides()));
    }
}
