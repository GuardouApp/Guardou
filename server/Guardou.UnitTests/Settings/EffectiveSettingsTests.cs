using Guardou.Application.Authentication;
using Guardou.Application.Settings;
using Guardou.Domain.Authentication;
using Guardou.Domain.Settings;
using Microsoft.Extensions.Options;

namespace Guardou.UnitTests.Settings;

public sealed class EffectiveSettingsTests
{
    private static readonly SettingsOverrides Overrides = new()
    {
        OidcIssuer = "https://id.example.com/",
        OidcClientSecret = "environment-secret",
        OidcScopes = "profile groups",
    };

    private readonly FakeSettingsStore _store = new();

    private static CancellationToken CancellationToken => TestContext.Current.CancellationToken;

    [Fact]
    public async Task EnvironmentVariablesOverrideTheStoredSettings()
    {
        await _store.SaveAsync(OidcSettings.Initial with { Issuer = "https://stored.example.com", ClientId = "guardou" }, CancellationToken);

        OidcSettings settings = await TestSettings.Oidc(_store, Overrides).GetAsync(CancellationToken);

        Assert.Equal("https://id.example.com", settings.Issuer);
        Assert.Equal("guardou", settings.ClientId);
        Assert.Equal("environment-secret", settings.ClientSecret);
        Assert.Equal(["openid", "profile", "groups"], settings.Scopes);
    }

    [Fact]
    public void ListsTheLockedFields()
    {
        IReadOnlyList<LockedSetting> locked = TestSettings.Oidc(_store, Overrides).Locked;

        Assert.Equal(
            [
                new LockedSetting("issuer", SettingsVariables.OidcIssuer),
                new LockedSetting("clientSecret", SettingsVariables.OidcClientSecret),
                new LockedSetting("scopes", SettingsVariables.OidcScopes),
            ],
            locked);
    }

    [Fact]
    public async Task DoesNotSaveTheEnvironmentValues()
    {
        await TestSettings.Oidc(_store, Overrides).SaveAsync(OidcSettings.Initial with { Issuer = "https://other.example.com", ClientId = "guardou" }, CancellationToken);

        OidcSettings stored = await _store.GetAsync<OidcSettings>(CancellationToken);
        Assert.Null(stored.Issuer);
        Assert.Null(stored.ClientSecret);
        Assert.Equal("guardou", stored.ClientId);
    }

    [Fact]
    public async Task DescribesTheSourceOfEachValueWithoutSecrets()
    {
        await _store.SaveAsync(new ServerSettings("https://files.example.com"), CancellationToken);
        EffectiveSettings<OidcSettings> oidc = TestSettings.Oidc(_store, Overrides);

        IReadOnlyList<EffectiveSetting> server = await TestSettings.Server(_store).DescribeAsync(CancellationToken);
        IReadOnlyList<EffectiveSetting> described = await oidc.DescribeAsync(CancellationToken);

        Assert.Equal(new EffectiveSetting("server.externalUrl", "https://files.example.com", false, SettingSource.Database, SettingsVariables.ExternalUrl), server[0]);
        Assert.Equal(new EffectiveSetting("oidc.issuer", "https://id.example.com", false, SettingSource.Environment, SettingsVariables.OidcIssuer), described[0]);
        Assert.Equal(new EffectiveSetting("oidc.clientSecret", null, true, SettingSource.Environment, SettingsVariables.OidcClientSecret), described[2]);
        Assert.Equal(new EffectiveSetting("oidc.autoCreateUsers", "true", false, SettingSource.Default, null), described[5]);
    }

    [Theory]
    [InlineData("http://id.example.com", null)]
    [InlineData(null, 0)]
    public void RefusesInvalidEnvironmentValues(string? issuer, int? retention)
    {
        SettingsOverridesValidation validation = new(new ServerSettingsSchema(), new OidcSettingsSchema());

        ValidateOptionsResult result = validation.Validate(null, new SettingsOverrides { OidcIssuer = issuer, TrashRetentionDays = retention });

        Assert.True(result.Failed);
    }
}
