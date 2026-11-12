using System.Net;
using System.Net.Http.Json;
using Guardou.Application.Files;
using Guardou.Application.Tasks;

namespace Guardou.IntegrationTests;

public sealed class CopiesTests : SignedInTests
{
    [Fact]
    public async Task CopyAddsTheItemsBeforeTheBlobs()
    {
        ItemEntry file = await Client.UploadEntryAsync(ItemsApi.Root, "a.txt", "hello");
        ItemEntry folder = await Client.CreateFolderEntryAsync(ItemsApi.Root, "docs");

        ItemEntry copy = await ItemsApi.ReadEntryAsync(await Client.TransferAsync("copy", file.Id, folder.Id));
        IReadOnlyList<string> blobsBeforeTask = BlobFiles();
        HttpResponseMessage download = await Client.GetAsync($"/api/items/{copy.Id}/download", CancellationToken);
        HttpResponseMessage archive = await Client.GetAsync($"/api/items/{folder.Id}/download", CancellationToken);
        await RunCopyTasksAsync();

        Assert.True(copy.Processing);
        Assert.Single(blobsBeforeTask);
        Assert.Equal(HttpStatusCode.Conflict, download.StatusCode);
        Assert.Equal([CopyPendingException.Code], await UsersApi.ReadErrorCodesAsync(download));
        Assert.Equal(HttpStatusCode.Conflict, archive.StatusCode);
        Assert.False((await Client.GetItemAsync(copy.Id)).Entry.Processing);
        Assert.Equal("hello", (await Client.GetItemAsync(copy.Id, "?content=true")).Content);
        Assert.Equal(2, BlobFiles().Count);
    }

    [Fact]
    public async Task CopyOfAProcessingCopyReadsTheFirstSource()
    {
        ItemEntry file = await Client.UploadEntryAsync(ItemsApi.Root, "a.txt", "hello");
        ItemEntry folder = await Client.CreateFolderEntryAsync(ItemsApi.Root, "docs");
        ItemEntry first = await ItemsApi.ReadEntryAsync(await Client.TransferAsync("copy", file.Id, folder.Id));

        ItemEntry second = await ItemsApi.ReadEntryAsync(await Client.TransferAsync("copy", first.Id, ItemsApi.Root, ConflictPolicy.Rename));
        await RunCopyTasksAsync();

        Assert.Equal("hello", (await Client.GetItemAsync(second.Id, "?content=true")).Content);
        Assert.Equal(3, BlobFiles().Count);
    }

    [Fact]
    public async Task CopyWithoutSourceGoesToTheTrash()
    {
        ItemEntry file = await Client.UploadEntryAsync(ItemsApi.Root, "a.txt", "hello");
        ItemEntry folder = await Client.CreateFolderEntryAsync(ItemsApi.Root, "docs");
        ItemEntry copy = await ItemsApi.ReadEntryAsync(await Client.TransferAsync("copy", file.Id, folder.Id));
        (await Client.PostSelectionAsync("/api/items/trash", file.Id)).EnsureSuccessStatusCode();
        (await Client.PostSelectionAsync("/api/trash/delete", file.Id)).EnsureSuccessStatusCode();

        await RunCopyTasksAsync();
        IReadOnlyList<TrashEntry>? trash = await Client.GetFromJsonAsync<IReadOnlyList<TrashEntry>>(
            "/api/trash",
            HttpClientExtensions.JsonOptions,
            CancellationToken);

        Assert.Equal([copy.Id], trash?.Select(entry => entry.Entry.Id));
        Assert.Empty(await Client.ListTasksAsync("?kind=copy"));
        Assert.Empty(BlobFiles());
    }

    private async Task RunCopyTasksAsync()
    {
        foreach (TaskEntry task in await Client.ListTasksAsync("?kind=copy"))
        {
            (await Client.PostAsync($"/api/tasks/{task.Id}/run", null, CancellationToken)).EnsureSuccessStatusCode();
        }
    }

    private protected override TestServerFactory CreateFactory()
    {
        return TestServerFactory.WithoutTaskWorkers();
    }
}
