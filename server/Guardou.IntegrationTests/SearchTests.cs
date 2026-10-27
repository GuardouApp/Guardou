using System.Net;
using System.Net.Http.Json;
using Guardou.Application.Files;
using Guardou.Domain.Files;
using Guardou.Infrastructure.Indexing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Guardou.IntegrationTests;

public sealed class SearchTests : SignedInTests
{
    [Fact]
    public async Task FindsNamesThatContainTheTerms()
    {
        ItemEntry docs = await Client.CreateFolderEntryAsync(ItemsApi.Root, "docs");
        ItemEntry report = await Client.UploadEntryAsync(docs.Id, "Annual Report.pdf", "1");
        await Client.UploadEntryAsync(ItemsApi.Root, "notes.txt", "2");

        Assert.Equal([report.Id], await SearchIdsAsync(Client, "report annual"));
        Assert.Equal([report.Id], await SearchIdsAsync(Client, "\"ual rep\""));
        Assert.Empty(await SearchIdsAsync(Client, "missing"));
    }

    [Fact]
    public async Task FiltersByTypeAndSize()
    {
        ItemEntry photos = await Client.CreateFolderEntryAsync(ItemsApi.Root, "photos");
        ItemEntry small = await Client.UploadEntryAsync(photos.Id, "small.png", "x");
        ItemEntry large = await Client.UploadEntryAsync(photos.Id, "large.png", new string('x', 2048));
        await Client.UploadEntryAsync(photos.Id, "list.txt", new string('x', 2048));

        Assert.Equal(ItemType.Image, large.Type);
        Assert.Equal(ItemType.Text, (await SearchAsync(Client, "list")).Single().Type);
        Assert.Equal([photos.Id], await SearchIdsAsync(Client, "type:folder"));
        Assert.Equal([large.Id], await SearchIdsAsync(Client, "type:image size:>1KB"));
        Assert.Equal([small.Id], await SearchIdsAsync(Client, "type:image size:<1KB"));
    }

    [Fact]
    public async Task FiltersByModifiedDate()
    {
        ItemEntry recent = await Client.UploadEntryAsync(ItemsApi.Root, "recent.txt", "1");
        await ReadEntryAsync(Client.UploadAsync(ItemsApi.Root, "old.txt", "2", modified: new DateTimeOffset(2020, 5, 1, 0, 0, 0, TimeSpan.Zero)));

        Assert.Equal([recent.Id], await SearchIdsAsync(Client, "txt modified:7d"));
        Assert.Single(await SearchIdsAsync(Client, "txt modified:2020"));
    }

    [Fact]
    public async Task SearchesTheCurrentFolder()
    {
        ItemEntry docs = await Client.CreateFolderEntryAsync(ItemsApi.Root, "docs");
        ItemEntry inner = await Client.CreateFolderEntryAsync(docs.Id, "inner");
        ItemEntry inside = await Client.UploadEntryAsync(inner.Id, "plan.txt", "1");
        await Client.UploadEntryAsync(ItemsApi.Root, "plan.md", "2");

        Assert.Equal([inside.Id], await SearchIdsAsync(Client, "plan", docs.Id));
        Assert.Empty(await SearchIdsAsync(Client, "docs", docs.Id));
    }

    [Fact]
    public async Task FollowsRenamesMovesAndTrash()
    {
        ItemEntry docs = await Client.CreateFolderEntryAsync(ItemsApi.Root, "docs");
        ItemEntry archive = await Client.CreateFolderEntryAsync(ItemsApi.Root, "archive");
        ItemEntry file = await Client.UploadEntryAsync(docs.Id, "draft.txt", "1");

        (await Client.PatchAsJsonAsync($"/api/items/{file.Id}", new { name = "final.txt" }, CancellationToken)).EnsureSuccessStatusCode();
        Assert.Empty(await SearchIdsAsync(Client, "draft"));
        Assert.Equal([file.Id], await SearchIdsAsync(Client, "final"));

        (await Client.TransferAsync("move", docs.Id, archive.Id)).EnsureSuccessStatusCode();
        Assert.Equal([file.Id], await SearchIdsAsync(Client, "final", archive.Id));

        (await Client.PostSelectionAsync("/api/items/trash", docs.Id)).EnsureSuccessStatusCode();
        Assert.Empty(await SearchIdsAsync(Client, "final"));

        (await Client.PostSelectionAsync("/api/trash/restore", docs.Id)).EnsureSuccessStatusCode();
        Assert.Equal([file.Id], await SearchIdsAsync(Client, "final"));
    }

    [Fact]
    public async Task IncludesItemsSharedWithTheUser()
    {
        ItemEntry shared = await Client.CreateFolderEntryAsync(ItemsApi.Root, "shared");
        ItemEntry inside = await Client.UploadEntryAsync(shared.Id, "budget.txt", "1");
        await Client.UploadEntryAsync(ItemsApi.Root, "budget-private.txt", "2");
        string bobId = (await Client.FindUserAsync(BobUsername)).Id;
        (await Client.PutAsync($"/api/items/{shared.Id}/recipients/{bobId}", null, CancellationToken)).EnsureSuccessStatusCode();
        using HttpClient bob = await SignInAsBobAsync();

        IReadOnlyList<ItemEntry> results = await SearchAsync(bob, "budget");

        ItemEntry entry = Assert.Single(results);
        Assert.Equal(inside.Id, entry.Id);
        Assert.False(entry.Permissions.CanModify);
    }

    [Fact]
    public async Task LimitsTheResults()
    {
        for (int index = 0; index < 3; index++)
        {
            await Client.UploadEntryAsync(ItemsApi.Root, $"file-{index}.txt", "x");
        }

        Assert.Equal(2, (await SearchIdsAsync(Client, "file", limit: 2)).Count);
        Assert.Equal(HttpStatusCode.BadRequest, (await Client.GetAsync("/api/search?query=file&limit=0", CancellationToken)).StatusCode);
    }

    [Fact]
    public async Task RefusesAnInvalidFilter()
    {
        HttpResponseMessage response = await Client.GetAsync("/api/search?query=size:big", CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GivesRecursiveFolderSizes()
    {
        ItemEntry docs = await Client.CreateFolderEntryAsync(ItemsApi.Root, "docs");
        ItemEntry inner = await Client.CreateFolderEntryAsync(docs.Id, "inner");
        await Client.UploadEntryAsync(docs.Id, "a.txt", "1234");
        ItemEntry deep = await Client.UploadEntryAsync(inner.Id, "b.txt", "123456");

        Assert.Equal(10, (await Client.GetItemAsync(docs.Id)).Entry.Size);
        Assert.Equal(6, (await Client.GetItemAsync(inner.Id)).Entry.Size);

        (await Client.TransferAsync("copy", inner.Id, ItemsApi.Root)).EnsureSuccessStatusCode();
        Assert.Equal(16, (await Client.GetItemAsync(ItemsApi.Root)).Entry.Size);

        (await Client.PostSelectionAsync("/api/items/trash", deep.Id)).EnsureSuccessStatusCode();
        Assert.Equal(4, (await Client.GetItemAsync(docs.Id)).Entry.Size);

        await Client.UploadEntryAsync(inner.Id, "c.txt", "12");
        (await Client.TransferAsync("move", inner.Id, ItemsApi.Root, ConflictPolicy.Rename)).EnsureSuccessStatusCode();
        Assert.Equal(4, (await Client.GetItemAsync(docs.Id)).Entry.Size);
        Assert.Equal(12, (await Client.GetItemAsync(ItemsApi.Root)).Entry.Size);
    }

    [Fact]
    public async Task RemovesTheIndexOfADeletedUser()
    {
        string bobId = (await Client.FindUserAsync(BobUsername)).Id;
        using (HttpClient bob = await SignInAsBobAsync())
        {
            await bob.UploadEntryAsync(ItemsApi.Root, "bob.txt", "1");
        }

        (await Client.DeleteAsync($"/api/users/{bobId}", CancellationToken)).EnsureSuccessStatusCode();

        await using AsyncServiceScope scope = Factory.Services.CreateAsyncScope();
        Assert.False(await scope.ServiceProvider.GetRequiredService<IndexDbContext>().Entries.AnyAsync(entry => entry.OwnerId == bobId, CancellationToken));
    }

    [Fact]
    public async Task RebuildsAnEmptyIndex()
    {
        ItemEntry docs = await Client.CreateFolderEntryAsync(ItemsApi.Root, "docs");
        ItemEntry file = await Client.UploadEntryAsync(docs.Id, "rebuilt.txt", "123");
        await using (AsyncServiceScope scope = Factory.Services.CreateAsyncScope())
        {
            await scope.ServiceProvider.GetRequiredService<IndexDbContext>().Entries.ExecuteDeleteAsync(CancellationToken);
            await scope.ServiceProvider.GetRequiredService<ItemIndexWriter>().RebuildIfEmptyAsync(CancellationToken);
        }

        Assert.Equal([file.Id], await SearchIdsAsync(Client, "rebuilt"));
        Assert.Equal(3, (await Client.GetItemAsync(docs.Id)).Entry.Size);
    }

    private static async Task<ItemEntry> ReadEntryAsync(Task<HttpResponseMessage> response)
    {
        return await ItemsApi.ReadEntryAsync(await response);
    }

    private static async Task<IReadOnlyList<string>> SearchIdsAsync(HttpClient client, string query, string? folderId = null, int? limit = null)
    {
        return (await SearchAsync(client, query, folderId, limit)).Select(entry => entry.Id).ToList();
    }

    private static async Task<IReadOnlyList<ItemEntry>> SearchAsync(HttpClient client, string query, string? folderId = null, int? limit = null)
    {
        string url = $"/api/search?query={Uri.EscapeDataString(query)}"
            + (folderId is null ? string.Empty : $"&folderId={folderId}")
            + (limit is null ? string.Empty : $"&limit={limit}");
        return await client.GetFromJsonAsync<IReadOnlyList<ItemEntry>>(url, HttpClientExtensions.JsonOptions, CancellationToken)
            ?? throw new InvalidOperationException("The search results are missing.");
    }
}
