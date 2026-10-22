using System.Net;
using System.Net.Http.Json;
using Guardou.Application.Files;
using Guardou.Application.Shares;
using Guardou.Domain.Settings;

namespace Guardou.IntegrationTests;

public sealed class SharesTests : SignedInTests
{
    private const string LinkPrefix = "http://localhost/s/";

    [Fact]
    public async Task SharesAFileWithOneLink()
    {
        ItemEntry file = await Client.UploadEntryAsync(ItemsApi.Root, "report.txt", "0123456789");

        ShareLink first = await ShareAsync(file.Id);
        ShareLink second = await ShareAsync(file.Id);

        Assert.StartsWith(LinkPrefix, first.Url, StringComparison.Ordinal);
        Assert.Equal(first, second);
        Assert.Equal(first, await GetShareAsync(file.Id));
    }

    [Fact]
    public async Task UsesTheExternalUrlForTheLink()
    {
        ItemEntry file = await Client.UploadEntryAsync(ItemsApi.Root, "report.txt", "a");
        await Client.PutAsJsonAsync("/api/settings/server", new ServerSettings("https://files.example.com"), CancellationToken);

        ShareLink link = await ShareAsync(file.Id);

        Assert.StartsWith("https://files.example.com/s/", link.Url, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ReturnsNotFoundForAnItemThatIsNotShared()
    {
        ItemEntry file = await Client.UploadEntryAsync(ItemsApi.Root, "report.txt", "a");

        HttpResponseMessage response = await Client.GetAsync($"/api/items/{file.Id}/share", CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task RefusesToShareAFolder()
    {
        ItemEntry docs = await Client.CreateFolderEntryAsync(ItemsApi.Root, "docs");

        HttpResponseMessage response = await Client.PutAsync($"/api/items/{docs.Id}/share", null, CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task DoesNotShareTheFileOfAnotherUser()
    {
        ItemEntry file = await Client.UploadEntryAsync(ItemsApi.Root, "report.txt", "a");
        using HttpClient bob = await SignInAsBobAsync();

        HttpResponseMessage response = await bob.PutAsync($"/api/items/{file.Id}/share", null, CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GivesAnonymousReadAccess()
    {
        ItemEntry file = await Client.UploadEntryAsync(ItemsApi.Root, "report.txt", "0123456789");
        string token = TokenOf(await ShareAsync(file.Id));
        using HttpClient anonymous = Factory.CreateClient();

        ItemEntry? shared = await anonymous.GetFromJsonAsync<ItemEntry>($"/api/shares/{token}", HttpClientExtensions.JsonOptions, CancellationToken);
        HttpResponseMessage download = await anonymous.GetAsync($"/api/shares/{token}/items/{file.Id}/download", CancellationToken);
        HttpResponseMessage view = await anonymous.GetAsync($"/api/shares/{token}/items/{file.Id}/view", CancellationToken);

        Assert.Equal(file.Id, shared?.Id);
        Assert.Null(shared?.ParentId);
        Assert.Equal(new ItemPermissions(CanModify: false, CanAddChildren: false), shared?.Permissions);
        Assert.Equal("0123456789", await download.Content.ReadAsStringAsync(CancellationToken));
        Assert.Equal("attachment", download.Content.Headers.ContentDisposition?.DispositionType);
        Assert.Equal("inline", view.Content.Headers.ContentDisposition?.DispositionType);
        Assert.Equal("sandbox", Assert.Single(view.Headers.GetValues("Content-Security-Policy")));
    }

    [Fact]
    public async Task RefusesAnotherItemThroughTheToken()
    {
        ItemEntry file = await Client.UploadEntryAsync(ItemsApi.Root, "report.txt", "a");
        ItemEntry other = await Client.UploadEntryAsync(ItemsApi.Root, "secret.txt", "b");
        string token = TokenOf(await ShareAsync(file.Id));
        using HttpClient anonymous = Factory.CreateClient();

        HttpResponseMessage response = await anonymous.GetAsync($"/api/shares/{token}/items/{other.Id}/download", CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Theory]
    [InlineData("unknown")]
    [InlineData("AAAAAAAAAAAAAAAAAAAAAA")]
    public async Task ReturnsNotFoundForAnUnknownToken(string token)
    {
        using HttpClient anonymous = Factory.CreateClient();

        HttpResponseMessage response = await anonymous.GetAsync($"/api/shares/{token}", CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task StopsSharingAndGivesANewTokenNextTime()
    {
        ItemEntry file = await Client.UploadEntryAsync(ItemsApi.Root, "report.txt", "a");
        string token = TokenOf(await ShareAsync(file.Id));

        HttpResponseMessage stop = await Client.DeleteAsync($"/api/items/{file.Id}/share", CancellationToken);
        HttpResponseMessage old = await Factory.CreateClient().GetAsync($"/api/shares/{token}", CancellationToken);
        string renewed = TokenOf(await ShareAsync(file.Id));

        Assert.Equal(HttpStatusCode.NoContent, stop.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, old.StatusCode);
        Assert.NotEqual(token, renewed);
    }

    [Fact]
    public async Task DeletesTheSharesOfFilesInATrashedFolder()
    {
        ItemEntry docs = await Client.CreateFolderEntryAsync(ItemsApi.Root, "docs");
        ItemEntry file = await Client.UploadEntryAsync(docs.Id, "report.txt", "a");
        string token = TokenOf(await ShareAsync(file.Id));

        await Client.PostSelectionAsync("/api/items/trash", docs.Id);
        await Client.PostSelectionAsync("/api/trash/restore", docs.Id);

        Assert.Equal(HttpStatusCode.NotFound, (await Factory.CreateClient().GetAsync($"/api/shares/{token}", CancellationToken)).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await Client.GetAsync($"/api/items/{file.Id}/share", CancellationToken)).StatusCode);
    }

    [Fact]
    public async Task DeletesTheShareOfAReplacedFile()
    {
        ItemEntry file = await Client.UploadEntryAsync(ItemsApi.Root, "report.txt", "a");
        string token = TokenOf(await ShareAsync(file.Id));

        await Client.UploadAsync(ItemsApi.Root, "report.txt", "b", ConflictPolicy.Replace);

        Assert.Equal(HttpStatusCode.NotFound, (await Factory.CreateClient().GetAsync($"/api/shares/{token}", CancellationToken)).StatusCode);
    }

    [Fact]
    public async Task DeletesTheShareWithTheFile()
    {
        ItemEntry file = await Client.UploadEntryAsync(ItemsApi.Root, "report.txt", "a");
        await ShareAsync(file.Id);
        await Client.PostSelectionAsync("/api/items/trash", file.Id);

        HttpResponseMessage response = await Client.PostSelectionAsync("/api/trash/delete", file.Id);

        Assert.True(response.IsSuccessStatusCode);
    }

    private static string TokenOf(ShareLink link)
    {
        return link.Url[(link.Url.LastIndexOf('/') + 1)..];
    }

    private async Task<ShareLink> ShareAsync(string id)
    {
        HttpResponseMessage response = await Client.PutAsync($"/api/items/{id}/share", null, CancellationToken);
        response.EnsureSuccessStatusCode();
        return await ReadLinkAsync(response);
    }

    private async Task<ShareLink> GetShareAsync(string id)
    {
        HttpResponseMessage response = await Client.GetAsync($"/api/items/{id}/share", CancellationToken);
        response.EnsureSuccessStatusCode();
        return await ReadLinkAsync(response);
    }

    private static async Task<ShareLink> ReadLinkAsync(HttpResponseMessage response)
    {
        return await response.Content.ReadFromJsonAsync<ShareLink>(HttpClientExtensions.JsonOptions, CancellationToken)
            ?? throw new InvalidOperationException("The share link is missing.");
    }
}
