using System.Net;
using System.Net.Http.Headers;
using Guardou.Application.Files;

namespace Guardou.IntegrationTests;

public sealed class PreviewsTests : SignedInTests
{
    [Fact]
    public async Task CreatesWebpPreviewsAfterAnUpload()
    {
        ItemEntry image = await Client.UploadImageAsync("wide.png");

        HttpResponseMessage thumbnail = await Client.WaitForPreviewAsync(image.Id);
        HttpResponseMessage large = await Client.WaitForPreviewAsync(image.Id, "large");

        Assert.True(image.HasPreview);
        Assert.Equal(HttpStatusCode.OK, thumbnail.StatusCode);
        Assert.Equal("image/webp", thumbnail.Content.Headers.ContentType?.MediaType);
        Assert.Equal(HttpStatusCode.OK, large.StatusCode);
        Assert.NotEqual(thumbnail.Headers.ETag, large.Headers.ETag);
        Assert.True(thumbnail.Headers.CacheControl?.Private);
        Assert.True(thumbnail.Headers.CacheControl?.NoCache);
    }

    [Fact]
    public async Task ListsThePreviewIdAndCachesPreviewsByIdForever()
    {
        ItemEntry image = await Client.UploadImageAsync("wide.png");
        await Client.WaitForPreviewAsync(image.Id);

        string? previewId = (await Client.GetItemAsync(image.Id)).Entry.PreviewId;
        HttpResponseMessage response = await Client.GetAsync(
            $"/api/items/{image.Id}/previews/{previewId}?size=thumbnail",
            CancellationToken);

        Assert.Null(image.PreviewId);
        Assert.NotNull(previewId);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("image/webp", response.Content.Headers.ContentType?.MediaType);
        Assert.Equal(TimeSpan.FromDays(365), response.Headers.CacheControl?.MaxAge);
        Assert.Contains("immutable", response.Headers.CacheControl?.Extensions.Select(extension => extension.Name) ?? []);
    }

    [Fact]
    public async Task GivesANewPreviewIdAfterARegenerationAndRemovesTheOldOne()
    {
        ItemEntry image = await Client.UploadImageAsync("wide.png");
        await Client.WaitForPreviewAsync(image.Id);
        string? before = (await Client.GetItemAsync(image.Id)).Entry.PreviewId;

        await Client.PostAsync("/api/tasks/queues/preview/all", null, CancellationToken);
        string? after = await WaitForNewPreviewIdAsync(image.Id, before);
        HttpResponseMessage old = await Client.GetAsync($"/api/items/{image.Id}/previews/{before}?size=thumbnail", CancellationToken);

        Assert.NotNull(after);
        Assert.NotEqual(before, after);
        Assert.Equal(HttpStatusCode.NotFound, old.StatusCode);
        Assert.Equal(2, CacheFiles().Count);
    }

    [Fact]
    public async Task ReturnsNotModifiedForTheSameEntityTag()
    {
        ItemEntry image = await Client.UploadImageAsync("wide.png");
        EntityTagHeaderValue? entityTag = (await Client.WaitForPreviewAsync(image.Id)).Headers.ETag;

        using HttpRequestMessage request = new(HttpMethod.Get, $"/api/items/{image.Id}/preview?size=thumbnail");
        request.Headers.IfNoneMatch.Add(entityTag ?? EntityTagHeaderValue.Any);
        HttpResponseMessage response = await Client.SendAsync(request, CancellationToken);

        Assert.Equal(HttpStatusCode.NotModified, response.StatusCode);
    }

    [Fact]
    public async Task HasNoPreviewForOtherFiles()
    {
        ItemEntry text = await Client.UploadEntryAsync(ItemsApi.Root, "notes.txt", "text");
        ItemEntry folder = await Client.CreateFolderEntryAsync(ItemsApi.Root, "docs");

        Assert.False(text.HasPreview);
        Assert.False(folder.HasPreview);
        Assert.Equal(HttpStatusCode.NotFound, (await Client.GetPreviewAsync(text.Id)).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await Client.GetPreviewAsync(folder.Id)).StatusCode);
    }

    [Fact]
    public async Task HidesThePreviewsOfOtherUsers()
    {
        ItemEntry image = await Client.UploadImageAsync("wide.png");
        await Client.WaitForPreviewAsync(image.Id);
        using HttpClient bob = await SignInAsBobAsync();

        HttpResponseMessage response = await bob.GetPreviewAsync(image.Id);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task ReturnsNotFoundWhenThePreviewFails()
    {
        ItemEntry broken = await ItemsApi.ReadEntryAsync(await Client.UploadAsync(ItemsApi.Root, "broken.png", "not an image"));

        HttpResponseMessage response = await Client.WaitForPreviewAsync(broken.Id);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CreatesPreviewsForCopies()
    {
        ItemEntry image = await Client.UploadImageAsync("wide.png");
        ItemEntry folder = await Client.CreateFolderEntryAsync(ItemsApi.Root, "copies");

        ItemEntry copy = await ItemsApi.ReadEntryAsync(await Client.TransferAsync("copy", image.Id, folder.Id));

        Assert.Equal(HttpStatusCode.OK, (await Client.WaitForPreviewAsync(copy.Id)).StatusCode);
    }

    [Fact]
    public async Task DeletesPreviewsWithTheFile()
    {
        ItemEntry image = await Client.UploadImageAsync("wide.png");
        await Client.WaitForPreviewAsync(image.Id);
        await Client.WaitForPreviewAsync(image.Id, "large");
        int cached = CacheFiles().Count;

        await Client.PostSelectionAsync("/api/items/trash", image.Id);
        await Client.PostSelectionAsync("/api/trash/delete", image.Id);

        Assert.Equal(2, cached);
        Assert.Empty(CacheFiles());
    }

    private async Task<string?> WaitForNewPreviewIdAsync(string id, string? previous)
    {
        using CancellationTokenSource timeout = CancellationTokenSource.CreateLinkedTokenSource(CancellationToken);
        timeout.CancelAfter(TimeSpan.FromSeconds(30));
        while ((await Client.GetItemAsync(id)).Entry.PreviewId is var current && (current is null || current == previous))
        {
            await Task.Delay(TimeSpan.FromMilliseconds(100), timeout.Token);
        }

        return (await Client.GetItemAsync(id)).Entry.PreviewId;
    }

    private List<string> CacheFiles()
    {
        return Directory.Exists(Factory.CacheDirectory)
            ? Directory.EnumerateFiles(Factory.CacheDirectory, "*", SearchOption.AllDirectories).ToList()
            : [];
    }
}
