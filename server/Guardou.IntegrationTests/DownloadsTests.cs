using System.IO.Compression;
using System.Net;
using System.Net.Http.Headers;
using Guardou.Application.Files;

namespace Guardou.IntegrationTests;

public sealed class DownloadsTests : SignedInTests
{
    [Fact]
    public async Task DownloadsAFileWithItsName()
    {
        ItemEntry file = await Client.UploadEntryAsync(ItemsApi.Root, "report.txt", "0123456789");

        HttpResponseMessage response = await Client.GetAsync($"/api/items/{file.Id}/download", CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("attachment", response.Content.Headers.ContentDisposition?.DispositionType);
        Assert.Equal("report.txt", response.Content.Headers.ContentDisposition?.FileNameStar);
        Assert.Equal("0123456789", await response.Content.ReadAsStringAsync(CancellationToken));
    }

    [Fact]
    public async Task DownloadsARange()
    {
        ItemEntry file = await Client.UploadEntryAsync(ItemsApi.Root, "report.txt", "0123456789");
        using HttpRequestMessage request = new(HttpMethod.Get, $"/api/items/{file.Id}/download");
        request.Headers.Range = new RangeHeaderValue(2, 5);

        HttpResponseMessage response = await Client.SendAsync(request, CancellationToken);

        Assert.Equal(HttpStatusCode.PartialContent, response.StatusCode);
        Assert.Equal("2345", await response.Content.ReadAsStringAsync(CancellationToken));
    }

    [Fact]
    public async Task ViewsAFileInline()
    {
        ItemEntry file = await Client.UploadEntryAsync(ItemsApi.Root, "page.html", "<p>hi</p>");

        HttpResponseMessage response = await Client.GetAsync($"/api/items/{file.Id}/view", CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("inline", response.Content.Headers.ContentDisposition?.DispositionType);
        Assert.Equal("text/html", response.Content.Headers.ContentType?.MediaType);
        Assert.Equal("sandbox", Assert.Single(response.Headers.GetValues("Content-Security-Policy")));
        Assert.Equal("nosniff", Assert.Single(response.Headers.GetValues("X-Content-Type-Options")));
    }

    [Fact]
    public async Task ViewsAPdfWithoutTheSandbox()
    {
        ItemEntry file = await Client.UploadEntryAsync(ItemsApi.Root, "manual.pdf", "%PDF-1.4");

        HttpResponseMessage response = await Client.GetAsync($"/api/items/{file.Id}/view", CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/pdf", response.Content.Headers.ContentType?.MediaType);
        Assert.False(response.Headers.Contains("Content-Security-Policy"));
        Assert.Equal("nosniff", Assert.Single(response.Headers.GetValues("X-Content-Type-Options")));
    }

    [Fact]
    public async Task DownloadsAFolderAsAZip()
    {
        ItemEntry docs = await Client.CreateFolderEntryAsync(ItemsApi.Root, "docs");
        await Client.CreateFolderEntryAsync(docs.Id, "empty");
        await Client.UploadEntryAsync(docs.Id, "a.txt", "a");

        HttpResponseMessage response = await Client.GetAsync($"/api/items/{docs.Id}/download", CancellationToken);

        Assert.Equal("application/zip", response.Content.Headers.ContentType?.MediaType);
        Assert.Equal("docs.zip", response.Content.Headers.ContentDisposition?.FileNameStar);
        Assert.Equal(["docs/", "docs/a.txt", "docs/empty/"], await EntriesAsync(response));
    }

    [Fact]
    public async Task DownloadsASelectionAsAZip()
    {
        ItemEntry first = await Client.UploadEntryAsync(ItemsApi.Root, "a.txt", "a");
        ItemEntry second = await Client.UploadEntryAsync(ItemsApi.Root, "b.txt", "b");

        HttpResponseMessage response = await Client.GetAsync($"/api/items/archive?ids={first.Id}&ids={second.Id}", CancellationToken);
        HttpResponseMessage empty = await Client.GetAsync("/api/items/archive", CancellationToken);

        Assert.Equal("download.zip", response.Content.Headers.ContentDisposition?.FileNameStar);
        Assert.Equal(["a.txt", "b.txt"], await EntriesAsync(response));
        Assert.Equal(HttpStatusCode.BadRequest, empty.StatusCode);
    }

    private static async Task<IReadOnlyList<string>> EntriesAsync(HttpResponseMessage response)
    {
        await using Stream content = await response.Content.ReadAsStreamAsync(CancellationToken);
        using MemoryStream buffer = new();
        await content.CopyToAsync(buffer, CancellationToken);
        await using ZipArchive zip = new(buffer, ZipArchiveMode.Read);
        return zip.Entries.Select(entry => entry.FullName).Order(StringComparer.Ordinal).ToList();
    }
}
