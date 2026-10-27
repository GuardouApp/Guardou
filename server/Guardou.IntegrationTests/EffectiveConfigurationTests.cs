using System.Net;
using System.Net.Http.Json;
using Guardou.Application.Files;
using Guardou.Application.Settings;
using Guardou.Domain.Settings;

namespace Guardou.IntegrationTests;

public sealed class EffectiveConfigurationTests : SignedInTests
{
    private const string EnvironmentUrl = "https://env.example.com";

    [Fact]
    public async Task ShowsTheEffectiveValuesWithoutSecrets()
    {
        IReadOnlyList<EffectiveSetting> settings = await GetAsync<IReadOnlyList<EffectiveSetting>>("/api/settings/effective");

        Assert.Contains(new EffectiveSetting("server.externalUrl", EnvironmentUrl, false, SettingSource.Environment, SettingsVariables.ExternalUrl), settings);
        Assert.Contains(new EffectiveSetting("server.trashRetentionDays", "7", false, SettingSource.Environment, SettingsVariables.TrashRetentionDays), settings);
        Assert.Contains(new EffectiveSetting("oidc.clientSecret", null, true, SettingSource.Environment, SettingsVariables.OidcClientSecret), settings);
        Assert.Contains(new EffectiveSetting("oidc.userClaim", "preferred_username", false, SettingSource.Default, SettingsVariables.OidcUserClaim), settings);
    }

    [Fact]
    public async Task KeepsTheLockedFields()
    {
        HttpResponseMessage response = await Client.PutAsJsonAsync("/api/settings/server", new ServerSettings("https://other.example.com", 9), HttpClientExtensions.JsonOptions, CancellationToken);
        ServerSettingsView view = await GetAsync<ServerSettingsView>("/api/settings/server");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(EnvironmentUrl, view.ExternalUrl);
        Assert.Equal(7, view.TrashRetentionDays);
        Assert.Equal(
            [new LockedSetting("externalUrl", SettingsVariables.ExternalUrl), new LockedSetting("trashRetentionDays", SettingsVariables.TrashRetentionDays)],
            view.Locked);
    }

    [Fact]
    public async Task UsesTheTrashRetention()
    {
        ItemEntry file = await Client.UploadEntryAsync(ItemsApi.Root, "a.txt", "a");
        await Client.PostSelectionAsync("/api/items/trash", file.Id);

        IReadOnlyList<TrashEntry> trash = await GetAsync<IReadOnlyList<TrashEntry>>("/api/trash");

        Assert.All(trash, entry => Assert.Equal(entry.Trashed + TimeSpan.FromDays(7), entry.Expires));
    }

    [Fact]
    public async Task RefusesUsersWithoutTheAdminType()
    {
        using HttpClient bob = await SignInAsBobAsync();

        HttpResponseMessage response = await bob.GetAsync("/api/settings/effective", CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    private protected override TestServerFactory CreateFactory()
    {
        return new TestServerFactory
        {
            Settings = new Dictionary<string, string>
            {
                [$"{SettingsOverrides.Section}:{nameof(SettingsOverrides.ExternalUrl)}"] = EnvironmentUrl,
                [$"{SettingsOverrides.Section}:{nameof(SettingsOverrides.TrashRetentionDays)}"] = "7",
                [$"{SettingsOverrides.Section}:{nameof(SettingsOverrides.OidcClientSecret)}"] = "secret",
            },
        };
    }

    private async Task<T> GetAsync<T>(string path)
    {
        return await Client.GetFromJsonAsync<T>(path, HttpClientExtensions.JsonOptions, CancellationToken)
            ?? throw new InvalidOperationException($"{path} returned no content.");
    }
}
