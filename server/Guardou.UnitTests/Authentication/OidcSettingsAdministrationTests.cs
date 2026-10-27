using Guardou.Application.Authentication;
using Guardou.Application.Settings;
using Guardou.Domain.Authentication;
using Guardou.Domain.Settings;
using Guardou.UnitTests.Settings;

namespace Guardou.UnitTests.Authentication;

public sealed class OidcSettingsAdministrationTests : IOidcSettingsObserver
{
    private const string RequestOrigin = "http://localhost:8080";

    private readonly FakeSettingsStore _store = new();
    private OidcSettings? _observed;

    private static CancellationToken CancellationToken => TestContext.Current.CancellationToken;

    private static OidcSettingsChange Change => new(
        Issuer: " https://id.example.com/realms/home/ ",
        ClientId: "guardou",
        ClientSecret: "secret",
        Scopes: ["profile email", " ", "groups", "email"],
        UserClaim: "preferred_username",
        AutoCreateUsers: true,
        PasswordLogin: true);

    void IOidcSettingsObserver.Changed(OidcSettings settings)
    {
        _observed = settings;
    }

    [Fact]
    public async Task NormalizesAndSavesTheSettings()
    {
        OidcSettingsView view = await Administration().ChangeAsync(Change, RequestOrigin, CancellationToken);

        OidcSettings saved = await _store.GetAsync<OidcSettings>(CancellationToken);
        Assert.Equal("https://id.example.com/realms/home", saved.Issuer);
        Assert.Equal(["openid", "profile", "email", "groups"], saved.Scopes);
        Assert.Same(saved, _observed);
        Assert.True(view.ClientSecretSet);
        Assert.Equal("http://localhost:8080/api/auth/oidc/callback", view.RedirectUri);
        Assert.Equal("http://localhost:8080/login", view.PostLogoutRedirectUri);
    }

    [Fact]
    public async Task KeepsTheSecretWhenItIsNotGiven()
    {
        await Administration().ChangeAsync(Change, RequestOrigin, CancellationToken);

        await Administration().ChangeAsync(Change with { ClientSecret = null }, RequestOrigin, CancellationToken);

        Assert.Equal("secret", (await _store.GetAsync<OidcSettings>(CancellationToken)).ClientSecret);
    }

    [Fact]
    public async Task ClearsTheSecretWhenItIsEmpty()
    {
        await Administration().ChangeAsync(Change, RequestOrigin, CancellationToken);

        OidcSettingsView view = await Administration().ChangeAsync(Change with { ClientSecret = string.Empty }, RequestOrigin, CancellationToken);

        Assert.False(view.ClientSecretSet);
    }

    [Fact]
    public async Task UsesTheExternalUrlForTheRedirectUris()
    {
        await _store.SaveAsync(new ServerSettings("https://files.example.com"), CancellationToken);

        OidcSettingsView view = await Administration().GetAsync(RequestOrigin, CancellationToken);

        Assert.Equal("https://files.example.com/api/auth/oidc/callback", view.RedirectUri);
    }

    [Fact]
    public async Task ReturnsTheDefaultsWithoutSavingThem()
    {
        await Administration().ChangeAsync(Change, RequestOrigin, CancellationToken);

        OidcSettingsView defaults = await Administration().DefaultsAsync(RequestOrigin, CancellationToken);

        Assert.Null(defaults.Issuer);
        Assert.Equal(OidcSettings.Initial.Scopes, defaults.Scopes);
        Assert.Equal("guardou", (await _store.GetAsync<OidcSettings>(CancellationToken)).ClientId);
    }

    [Theory]
    [InlineData("ftp://id.example.com", "guardou")]
    [InlineData("http://id.example.com", "guardou")]
    [InlineData("https://id.example.com?realm=home", "guardou")]
    [InlineData("https://id.example.com", " ")]
    [InlineData(null, "guardou")]
    public async Task RefusesAnInvalidClient(string? issuer, string clientId)
    {
        await Assert.ThrowsAsync<InvalidSettingsException>(() =>
            Administration().ChangeAsync(Change with { Issuer = issuer, ClientId = clientId }, RequestOrigin, CancellationToken));
    }

    [Fact]
    public async Task RefusesToDisableThePasswordLoginWithoutOidc()
    {
        OidcSettingsChange change = Change with { Issuer = null, ClientId = null, PasswordLogin = false };

        await Assert.ThrowsAsync<InvalidSettingsException>(() => Administration().ChangeAsync(change, RequestOrigin, CancellationToken));
    }

    [Fact]
    public async Task EnablesThePasswordLoginAgain()
    {
        await Administration().ChangeAsync(Change with { PasswordLogin = false }, RequestOrigin, CancellationToken);

        await Administration().EnablePasswordLoginAsync(CancellationToken);

        Assert.True((await _store.GetAsync<OidcSettings>(CancellationToken)).PasswordLogin);
    }

    private OidcSettingsAdministration Administration()
    {
        return new OidcSettingsAdministration(_store, new ServerSettingsAdministration(_store), this);
    }
}
