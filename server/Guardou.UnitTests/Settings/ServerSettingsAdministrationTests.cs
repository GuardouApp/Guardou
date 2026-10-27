using Guardou.Application.Settings;
using Guardou.Domain.Settings;

namespace Guardou.UnitTests.Settings;

public sealed class ServerSettingsAdministrationTests
{
    private readonly FakeSettingsStore _store = new();

    private static CancellationToken CancellationToken => TestContext.Current.CancellationToken;

    [Theory]
    [InlineData(" https://files.example.com/ ", "https://files.example.com")]
    [InlineData("http://192.168.1.10:8080", "http://192.168.1.10:8080")]
    [InlineData("", null)]
    [InlineData(null, null)]
    public async Task NormalizesTheExternalUrl(string? value, string? expected)
    {
        ServerSettingsView settings = await Administration().ChangeAsync(new ServerSettings(value), CancellationToken);

        Assert.Equal(expected, settings.ExternalUrl);
        Assert.Equal(expected, (await _store.GetAsync<ServerSettings>(CancellationToken)).ExternalUrl);
    }

    [Theory]
    [InlineData("files.example.com")]
    [InlineData("https://user@files.example.com")]
    [InlineData("https://files.example.com/#top")]
    public async Task RefusesAnInvalidExternalUrl(string value)
    {
        await Assert.ThrowsAsync<InvalidSettingsException>(() =>
            Administration().ChangeAsync(new ServerSettings(value), CancellationToken));
    }

    [Fact]
    public async Task ChangesTheTrashRetention()
    {
        ServerSettingsView settings = await Administration().ChangeAsync(new ServerSettings(null, 7), CancellationToken);

        Assert.Equal(7, settings.TrashRetentionDays);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(3651)]
    public async Task RefusesATrashRetentionOutOfRange(int days)
    {
        await Assert.ThrowsAsync<InvalidSettingsException>(() =>
            Administration().ChangeAsync(new ServerSettings(null, days), CancellationToken));
    }

    [Fact]
    public async Task UsesTheRequestOriginWithoutAnExternalUrl()
    {
        Assert.Equal("http://localhost", await Administration().PublicOriginAsync("http://localhost", CancellationToken));
    }

    [Fact]
    public async Task KeepsTheStoredValueOfALockedField()
    {
        await _store.SaveAsync(new ServerSettings("https://stored.example.com"), CancellationToken);
        ServerSettingsAdministration administration = Administration(new SettingsOverrides { ExternalUrl = "https://env.example.com" });

        ServerSettingsView view = await administration.ChangeAsync(new ServerSettings("https://other.example.com", 9), CancellationToken);

        Assert.Equal("https://env.example.com", view.ExternalUrl);
        Assert.Equal(9, view.TrashRetentionDays);
        Assert.Equal([new LockedSetting("externalUrl", SettingsVariables.ExternalUrl)], view.Locked);
        Assert.Equal(new ServerSettings("https://stored.example.com", 9), await _store.GetAsync<ServerSettings>(CancellationToken));
    }

    private ServerSettingsAdministration Administration(SettingsOverrides? overrides = null)
    {
        return new ServerSettingsAdministration(TestSettings.Server(_store, overrides));
    }
}
