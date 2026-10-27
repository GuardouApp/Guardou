using System.Net.Http.Json;
using Guardou.Application.Files;

namespace Guardou.IntegrationTests;

public sealed class RecentItemsTests : SignedInTests
{
    [Fact]
    public async Task ListsUploadedAndOpenedFilesNewestFirst()
    {
        ItemEntry docs = await Client.CreateFolderEntryAsync(ItemsApi.Root, "docs");
        ItemEntry first = await Client.UploadEntryAsync(docs.Id, "first.txt", "1");
        Factory.Time.Advance(TimeSpan.FromMinutes(1));
        ItemEntry second = await Client.UploadEntryAsync(ItemsApi.Root, "second.txt", "2");
        Factory.Time.Advance(TimeSpan.FromMinutes(1));
        await OpenAsync(Client, first.Id);

        IReadOnlyList<ItemEntry> recent = await GetRecentAsync(Client);

        Assert.Equal([first.Id, second.Id], recent.Select(entry => entry.Id));
    }

    [Fact]
    public async Task LimitsTheList()
    {
        for (int index = 0; index < RecentItems.Limit + 2; index++)
        {
            await Client.UploadEntryAsync(ItemsApi.Root, $"file-{index}.txt", "x");
            Factory.Time.Advance(TimeSpan.FromSeconds(1));
        }

        IReadOnlyList<ItemEntry> recent = await GetRecentAsync(Client);

        Assert.Equal(RecentItems.Limit, recent.Count);
        Assert.Equal($"file-{RecentItems.Limit + 1}.txt", recent[0].Name);
    }

    [Fact]
    public async Task OmitsTrashedFiles()
    {
        ItemEntry docs = await Client.CreateFolderEntryAsync(ItemsApi.Root, "docs");
        await Client.UploadEntryAsync(docs.Id, "inside.txt", "1");
        ItemEntry kept = await Client.UploadEntryAsync(ItemsApi.Root, "kept.txt", "2");
        await Client.PostSelectionAsync("/api/items/trash", docs.Id);

        IReadOnlyList<ItemEntry> recent = await GetRecentAsync(Client);

        Assert.Equal([kept.Id], recent.Select(entry => entry.Id));
    }

    [Fact]
    public async Task ListsOnlySharedFilesThatTheRecipientOpened()
    {
        ItemEntry opened = await Client.UploadEntryAsync(ItemsApi.Root, "opened.txt", "1");
        ItemEntry unopened = await Client.UploadEntryAsync(ItemsApi.Root, "unopened.txt", "2");
        string bobId = (await Client.FindUserAsync(BobUsername)).Id;
        await Client.PutAsync($"/api/items/{opened.Id}/recipients/{bobId}", null, CancellationToken);
        await Client.PutAsync($"/api/items/{unopened.Id}/recipients/{bobId}", null, CancellationToken);
        using HttpClient bob = await SignInAsBobAsync();
        await OpenAsync(bob, opened.Id);

        IReadOnlyList<ItemEntry> before = await GetRecentAsync(bob);
        await Client.DeleteAsync($"/api/items/{opened.Id}/recipients/{bobId}", CancellationToken);
        IReadOnlyList<ItemEntry> after = await GetRecentAsync(bob);

        ItemEntry entry = Assert.Single(before);
        Assert.Equal(opened.Id, entry.Id);
        Assert.False(entry.Permissions.CanModify);
        Assert.Empty(after);
    }

    private static async Task OpenAsync(HttpClient client, string id)
    {
        (await client.PutAsync($"/api/items/{id}/opened", null, CancellationToken)).EnsureSuccessStatusCode();
    }

    private static async Task<IReadOnlyList<ItemEntry>> GetRecentAsync(HttpClient client)
    {
        return await client.GetFromJsonAsync<IReadOnlyList<ItemEntry>>("/api/items/recent", HttpClientExtensions.JsonOptions, CancellationToken)
            ?? throw new InvalidOperationException("The recent items are missing.");
    }
}
