using System.Net;
using System.Net.Http.Json;
using Guardou.Application.Files;
using Guardou.Application.Users;

namespace Guardou.IntegrationTests;

public sealed class ItemStarsTests : SignedInTests
{
    [Fact]
    public async Task StarsAndUnstarsTheSelection()
    {
        ItemEntry docs = await Client.CreateFolderEntryAsync(ItemsApi.Root, "docs");
        ItemEntry report = await Client.UploadEntryAsync(docs.Id, "report.txt", "1");
        await Client.UploadEntryAsync(ItemsApi.Root, "notes.txt", "2");

        HttpResponseMessage star = await Client.PostSelectionAsync("/api/items/star", docs.Id, report.Id);
        IReadOnlyList<ItemEntry> starred = await StarredAsync(Client);
        HttpResponseMessage unstar = await Client.PostSelectionAsync("/api/items/unstar", docs.Id);

        Assert.False(docs.Starred);
        Assert.Equal(HttpStatusCode.NoContent, star.StatusCode);
        Assert.Equal(["docs", "report.txt"], starred.Select(entry => entry.Name));
        Assert.True(starred.All(entry => entry.Starred));
        Assert.Equal(HttpStatusCode.NoContent, unstar.StatusCode);
        Assert.Equal([report.Id], (await StarredAsync(Client)).Select(entry => entry.Id));
        Assert.True((await Client.GetItemAsync(report.Id)).Entry.Starred);
        Assert.False((await Client.GetItemAsync(docs.Id)).Entry.Starred);
    }

    [Fact]
    public async Task HidesTrashedItemsUntilRestore()
    {
        ItemEntry docs = await Client.CreateFolderEntryAsync(ItemsApi.Root, "docs");
        ItemEntry report = await Client.UploadEntryAsync(docs.Id, "report.txt", "1");
        await Client.PostSelectionAsync("/api/items/star", report.Id);

        await Client.PostSelectionAsync("/api/items/trash", docs.Id);
        IReadOnlyList<ItemEntry> trashed = await StarredAsync(Client);
        await Client.PostSelectionAsync("/api/trash/restore", docs.Id);

        Assert.Empty(trashed);
        Assert.Equal([report.Id], (await StarredAsync(Client)).Select(entry => entry.Id));
    }

    [Fact]
    public async Task RefusesTheRootFolder()
    {
        HttpResponseMessage star = await Client.PostSelectionAsync("/api/items/star", ItemsApi.Root);

        Assert.Equal(HttpStatusCode.BadRequest, star.StatusCode);
    }

    [Fact]
    public async Task KeepsStarsToTheOwner()
    {
        ItemEntry file = await Client.UploadEntryAsync(ItemsApi.Root, "report.txt", "1");
        await Client.PostSelectionAsync("/api/items/star", file.Id);
        UserAccount bob = await Client.FindUserAsync(BobUsername);
        await Client.PutAsync($"/api/items/{file.Id}/recipients/{bob.Id}", null, CancellationToken);
        using HttpClient bobClient = await SignInAsBobAsync();

        HttpResponseMessage star = await bobClient.PostSelectionAsync("/api/items/star", file.Id);

        Assert.Equal(HttpStatusCode.NotFound, star.StatusCode);
        Assert.False((await bobClient.GetItemAsync(file.Id)).Entry.Starred);
        Assert.Empty(await StarredAsync(bobClient));
    }

    [Fact]
    public async Task SearchesStarredItemsOfTheUser()
    {
        ItemEntry starred = await Client.UploadEntryAsync(ItemsApi.Root, "plan-a.txt", "1");
        await Client.UploadEntryAsync(ItemsApi.Root, "plan-b.txt", "2");
        await Client.PostSelectionAsync("/api/items/star", starred.Id);
        UserAccount bob = await Client.FindUserAsync(BobUsername);
        await Client.PutAsync($"/api/items/{starred.Id}/recipients/{bob.Id}", null, CancellationToken);
        using HttpClient bobClient = await SignInAsBobAsync();

        Assert.Equal([starred.Id], await SearchIdsAsync(Client, "plan is:starred"));
        Assert.Equal([starred.Id], await SearchIdsAsync(bobClient, "plan"));
        Assert.Empty(await SearchIdsAsync(bobClient, "is:starred"));
    }

    private static async Task<IReadOnlyList<ItemEntry>> StarredAsync(HttpClient client)
    {
        return await client.GetFromJsonAsync<IReadOnlyList<ItemEntry>>("/api/items/starred", HttpClientExtensions.JsonOptions, CancellationToken)
            ?? throw new InvalidOperationException("The starred items are missing.");
    }

    private static async Task<IReadOnlyList<string>> SearchIdsAsync(HttpClient client, string query)
    {
        IReadOnlyList<ItemEntry>? results = await client.GetFromJsonAsync<IReadOnlyList<ItemEntry>>(
            $"/api/search?query={Uri.EscapeDataString(query)}",
            HttpClientExtensions.JsonOptions,
            CancellationToken);
        return (results ?? []).Select(entry => entry.Id).ToList();
    }
}
