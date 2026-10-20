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
        ServerSettings settings = await new ServerSettingsAdministration(_store).ChangeAsync(new ServerSettings(value), CancellationToken);

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
            new ServerSettingsAdministration(_store).ChangeAsync(new ServerSettings(value), CancellationToken));
    }

    [Fact]
    public async Task UsesTheRequestOriginWithoutAnExternalUrl()
    {
        Assert.Equal("http://localhost", await new ServerSettingsAdministration(_store).PublicOriginAsync("http://localhost", CancellationToken));
    }
}
