using System.Net;
using System.Net.Http.Json;
using Guardou.Application.Files;
using Guardou.Domain.Files;
using Microsoft.Extensions.DependencyInjection;

namespace Guardou.IntegrationTests;

public sealed class TrashTests : SignedInTests
{
    [Fact]
    public async Task MovesItemsToTheTrash()
    {
        ItemEntry docs = await Client.CreateFolderEntryAsync(ItemsApi.Root, "docs");
        ItemEntry inner = await Client.UploadEntryAsync(docs.Id, "a.txt", "a");
        ItemEntry file = await Client.UploadEntryAsync(ItemsApi.Root, "b.txt", "b");

        HttpResponseMessage response = await Client.PostSelectionAsync("/api/items/trash", docs.Id, file.Id);
        IReadOnlyList<TrashEntry> trash = await GetTrashAsync();

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Empty((await Client.GetItemAsync(ItemsApi.Root)).Items ?? []);
        Assert.Equal(["b.txt", "docs"], trash.Select(entry => entry.Entry.Name).Order());
        Assert.All(trash, entry => Assert.Equal(entry.Trashed + TrashPolicy.Retention, entry.Expires));
        Assert.Equal(HttpStatusCode.NotFound, (await Client.GetAsync($"/api/items/{inner.Id}", CancellationToken)).StatusCode);
        Assert.Equal(2, BlobFiles().Count);
    }

    [Fact]
    public async Task DoesNotMoveTheRootToTheTrash()
    {
        ItemDetails root = await Client.GetItemAsync(ItemsApi.Root);

        HttpResponseMessage response = await Client.PostSelectionAsync("/api/items/trash", root.Entry.Id);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task RestoresItemsAndRenamesOnConflict()
    {
        ItemEntry docs = await Client.CreateFolderEntryAsync(ItemsApi.Root, "docs");
        ItemEntry file = await Client.UploadEntryAsync(docs.Id, "a.txt", "a");
        await Client.PostSelectionAsync("/api/items/trash", file.Id);
        await Client.UploadEntryAsync(docs.Id, "a.txt", "new");

        IReadOnlyList<ItemEntry>? restored = await (await Client.PostSelectionAsync("/api/trash/restore", file.Id))
            .Content.ReadFromJsonAsync<IReadOnlyList<ItemEntry>>(HttpClientExtensions.JsonOptions, CancellationToken);

        Assert.Equal(["a (1).txt"], restored?.Select(entry => entry.Name));
        Assert.Equal(["a (1).txt", "a.txt"], (await Client.GetItemAsync(docs.Id)).Names());
        Assert.Empty(await GetTrashAsync());
    }

    [Fact]
    public async Task RestoresToTheRootWhenTheParentIsGone()
    {
        ItemEntry docs = await Client.CreateFolderEntryAsync(ItemsApi.Root, "docs");
        ItemEntry file = await Client.UploadEntryAsync(docs.Id, "a.txt", "a");
        await Client.PostSelectionAsync("/api/items/trash", file.Id);
        await Client.PostSelectionAsync("/api/items/trash", docs.Id);

        await Client.PostSelectionAsync("/api/trash/restore", file.Id);

        Assert.Equal(["a.txt"], (await Client.GetItemAsync(ItemsApi.Root)).Names());
    }

    [Fact]
    public async Task DeletesItemsPermanently()
    {
        ItemEntry docs = await Client.CreateFolderEntryAsync(ItemsApi.Root, "docs");
        await Client.UploadEntryAsync(docs.Id, "a.txt", "a");
        ItemEntry active = await Client.UploadEntryAsync(ItemsApi.Root, "b.txt", "b");
        await Client.PostSelectionAsync("/api/items/trash", docs.Id);

        HttpResponseMessage notTrashed = await Client.PostSelectionAsync("/api/trash/delete", active.Id);
        HttpResponseMessage deleted = await Client.PostSelectionAsync("/api/trash/delete", docs.Id);

        Assert.Equal(HttpStatusCode.NotFound, notTrashed.StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, deleted.StatusCode);
        Assert.Empty(await GetTrashAsync());
        Assert.Single(BlobFiles());
    }

    [Fact]
    public async Task EmptiesTheTrash()
    {
        ItemEntry first = await Client.UploadEntryAsync(ItemsApi.Root, "a.txt", "a");
        ItemEntry second = await Client.UploadEntryAsync(ItemsApi.Root, "b.txt", "b");
        await Client.PostSelectionAsync("/api/items/trash", first.Id, second.Id);

        HttpResponseMessage response = await Client.DeleteAsync("/api/trash", CancellationToken);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Empty(await GetTrashAsync());
        Assert.Empty(BlobFiles());
    }

    [Fact]
    public async Task PurgesExpiredItems()
    {
        ItemEntry old = await Client.UploadEntryAsync(ItemsApi.Root, "old.txt", "a");
        await Client.PostSelectionAsync("/api/items/trash", old.Id);
        Factory.Time.Advance(TrashPolicy.Retention - TimeSpan.FromDays(1));
        await RenewSessionAsync();
        ItemEntry recent = await Client.UploadEntryAsync(ItemsApi.Root, "recent.txt", "b");
        await Client.PostSelectionAsync("/api/items/trash", recent.Id);
        Factory.Time.Advance(TimeSpan.FromDays(2));
        await RenewSessionAsync();

        await using (AsyncServiceScope scope = Factory.Services.CreateAsyncScope())
        {
            await scope.ServiceProvider.GetRequiredService<TrashService>().PurgeExpiredAsync(CancellationToken);
        }

        Assert.Equal(["recent.txt"], (await GetTrashAsync()).Select(entry => entry.Entry.Name));
        Assert.Single(BlobFiles());
    }

    private async Task<IReadOnlyList<TrashEntry>> GetTrashAsync()
    {
        return await Client.GetFromJsonAsync<IReadOnlyList<TrashEntry>>("/api/trash", HttpClientExtensions.JsonOptions, CancellationToken) ?? [];
    }
}
