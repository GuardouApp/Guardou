using System.Net;
using System.Net.Http.Json;
using Guardou.Application.Files;
using Guardou.Application.Shares;
using Guardou.Application.Users;

namespace Guardou.IntegrationTests;

public sealed class DirectSharesTests : SignedInTests
{
    [Fact]
    public async Task SharesAFileWithAUser()
    {
        ItemEntry file = await Client.UploadEntryAsync(ItemsApi.Root, "report.txt", "0123456789");
        UserAccount bob = await Client.FindUserAsync(BobUsername);

        IReadOnlyList<UserSummary> recipients = await AddRecipientAsync(file.Id, bob.Id);
        IReadOnlyList<UserSummary> again = await AddRecipientAsync(file.Id, bob.Id);

        UserSummary recipient = Assert.Single(recipients);
        Assert.Equal(new UserSummary(bob.Id, BobUsername, BobDisplayName, "bob@example.com", HasPicture: false), recipient);
        Assert.Equal(recipients, again);
        Assert.Equal(recipients, await GetRecipientsAsync(file.Id));
    }

    [Fact]
    public async Task GivesTheRecipientReadAccess()
    {
        ItemEntry file = await Client.UploadEntryAsync(ItemsApi.Root, "report.txt", "0123456789");
        await AddRecipientAsync(file.Id, (await Client.FindUserAsync(BobUsername)).Id);
        using HttpClient bob = await SignInAsBobAsync();

        ItemDetails details = await bob.GetItemAsync(file.Id);
        HttpResponseMessage download = await bob.GetAsync($"/api/items/{file.Id}/download", CancellationToken);
        HttpResponseMessage view = await bob.GetAsync($"/api/items/{file.Id}/view", CancellationToken);

        Assert.True(details.Shared);
        Assert.False((await Client.GetItemAsync(file.Id)).Shared);
        Assert.Null(details.Entry.ParentId);
        Assert.Empty(details.Ancestors);
        Assert.Equal(new ItemPermissions(CanModify: false, CanAddChildren: false), details.Entry.Permissions);
        Assert.Equal("0123456789", await download.Content.ReadAsStringAsync(CancellationToken));
        Assert.Equal(HttpStatusCode.OK, view.StatusCode);
    }

    [Fact]
    public async Task GivesTheRecipientReadAccessToAFolderTree()
    {
        ItemEntry docs = await Client.CreateFolderEntryAsync(ItemsApi.Root, "docs");
        ItemEntry inner = await Client.CreateFolderEntryAsync(docs.Id, "inner");
        ItemEntry file = await Client.UploadEntryAsync(inner.Id, "a.txt", "a");
        await AddRecipientAsync(docs.Id, (await Client.FindUserAsync(BobUsername)).Id);
        using HttpClient bob = await SignInAsBobAsync();

        ItemDetails root = await bob.GetItemAsync(docs.Id);
        ItemDetails folder = await bob.GetItemAsync(inner.Id);
        HttpResponseMessage children = await bob.GetAsync($"/api/items/{docs.Id}/children", CancellationToken);
        HttpResponseMessage archive = await bob.GetAsync($"/api/items/archive?ids={file.Id}&ids={inner.Id}", CancellationToken);

        Assert.Null(root.Entry.ParentId);
        Assert.Empty(root.Ancestors);
        Assert.Equal(["inner"], root.Names());
        Assert.Equal([docs.Id], folder.Ancestors.Select(crumb => crumb.Id));
        Assert.All(folder.Items ?? [], entry => Assert.False(entry.Permissions.CanModify));
        Assert.Equal(HttpStatusCode.OK, children.StatusCode);
        Assert.Equal("application/zip", archive.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task RefusesChangesByTheRecipient()
    {
        ItemEntry docs = await Client.CreateFolderEntryAsync(ItemsApi.Root, "docs");
        ItemEntry file = await Client.UploadEntryAsync(docs.Id, "a.txt", "a");
        await AddRecipientAsync(docs.Id, (await Client.FindUserAsync(BobUsername)).Id);
        using HttpClient bob = await SignInAsBobAsync();

        HttpResponseMessage folder = await bob.CreateFolderAsync(docs.Id, "new");
        HttpResponseMessage upload = await bob.UploadAsync(docs.Id, "b.txt", "b");
        HttpResponseMessage trash = await bob.PostSelectionAsync("/api/items/trash", file.Id);
        HttpResponseMessage share = await bob.PutAsync($"/api/items/{file.Id}/share", null, CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, folder.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, upload.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, trash.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, share.StatusCode);
    }

    [Fact]
    public async Task RefusesItemsOutsideTheSharedFolder()
    {
        ItemEntry docs = await Client.CreateFolderEntryAsync(ItemsApi.Root, "docs");
        ItemEntry secret = await Client.UploadEntryAsync(ItemsApi.Root, "secret.txt", "b");
        ItemDetails home = await Client.GetItemAsync(ItemsApi.Root);
        await AddRecipientAsync(docs.Id, (await Client.FindUserAsync(BobUsername)).Id);
        using HttpClient bob = await SignInAsBobAsync();

        HttpResponseMessage sibling = await bob.GetAsync($"/api/items/{secret.Id}", CancellationToken);
        HttpResponseMessage parent = await bob.GetAsync($"/api/items/{home.Entry.Id}/download", CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, sibling.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, parent.StatusCode);
    }

    [Fact]
    public async Task ListsTheItemsSharedWithMe()
    {
        ItemEntry docs = await Client.CreateFolderEntryAsync(ItemsApi.Root, "docs");
        ItemEntry file = await Client.UploadEntryAsync(docs.Id, "a.txt", "a");
        await Client.UploadEntryAsync(ItemsApi.Root, "private.txt", "b");
        string bobId = (await Client.FindUserAsync(BobUsername)).Id;
        await AddRecipientAsync(docs.Id, bobId);
        await AddRecipientAsync(file.Id, bobId);
        using HttpClient bob = await SignInAsBobAsync();

        IReadOnlyList<ItemEntry> shared = await GetSharedWithMeAsync(bob);

        Assert.Equal(["docs", "a.txt"], shared.Select(entry => entry.Name));
        Assert.All(shared, entry => Assert.Null(entry.ParentId));
        Assert.All(shared, entry => Assert.False(entry.Permissions.CanModify));
        Assert.Empty(await GetSharedWithMeAsync(Client));
    }

    [Fact]
    public async Task RemovesTheRecipient()
    {
        ItemEntry file = await Client.UploadEntryAsync(ItemsApi.Root, "report.txt", "a");
        string bobId = (await Client.FindUserAsync(BobUsername)).Id;
        await AddRecipientAsync(file.Id, bobId);
        using HttpClient bob = await SignInAsBobAsync();

        HttpResponseMessage removed = await Client.DeleteAsync($"/api/items/{file.Id}/recipients/{bobId}", CancellationToken);

        Assert.Equal(HttpStatusCode.NoContent, removed.StatusCode);
        Assert.Empty(await GetRecipientsAsync(file.Id));
        Assert.Empty(await GetSharedWithMeAsync(bob));
        Assert.Equal(HttpStatusCode.NotFound, (await bob.GetAsync($"/api/items/{file.Id}", CancellationToken)).StatusCode);
    }

    [Fact]
    public async Task DeletesTheDirectSharesOfATrashedFolder()
    {
        ItemEntry docs = await Client.CreateFolderEntryAsync(ItemsApi.Root, "docs");
        ItemEntry file = await Client.UploadEntryAsync(docs.Id, "a.txt", "a");
        await AddRecipientAsync(file.Id, (await Client.FindUserAsync(BobUsername)).Id);

        await Client.PostSelectionAsync("/api/items/trash", docs.Id);
        await Client.PostSelectionAsync("/api/trash/restore", docs.Id);

        Assert.Empty(await GetRecipientsAsync(file.Id));
    }

    [Fact]
    public async Task RefusesToShareWithTheOwner()
    {
        ItemEntry file = await Client.UploadEntryAsync(ItemsApi.Root, "report.txt", "a");
        UserAccount admin = await Client.FindUserAsync(TestServerFactory.AdminUsername);

        HttpResponseMessage response = await Client.PutAsync($"/api/items/{file.Id}/recipients/{admin.Id}", null, CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task RefusesAnUnknownUser()
    {
        ItemEntry file = await Client.UploadEntryAsync(ItemsApi.Root, "report.txt", "a");

        HttpResponseMessage response = await Client.PutAsync($"/api/items/{file.Id}/recipients/unknown", null, CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task RefusesToShareTheRootFolder()
    {
        string bobId = (await Client.FindUserAsync(BobUsername)).Id;

        HttpResponseMessage response = await Client.PutAsync($"/api/items/{ItemsApi.Root}/recipients/{bobId}", null, CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task ConvertsALinkShareIntoADirectShare()
    {
        ItemEntry docs = await Client.CreateFolderEntryAsync(ItemsApi.Root, "docs");
        string token = await ShareLinkAsync(docs.Id);
        UserAccount bobAccount = await Client.FindUserAsync(BobUsername);
        using HttpClient bob = await SignInAsBobAsync();

        ItemEntry joined = await JoinAsync(bob, token);
        await JoinAsync(bob, token);
        await Client.DeleteAsync($"/api/items/{docs.Id}/share", CancellationToken);

        Assert.Equal(docs.Id, joined.Id);
        Assert.Null(joined.ParentId);
        Assert.False(joined.Permissions.CanModify);
        Assert.Equal([bobAccount.Id], (await GetRecipientsAsync(docs.Id)).Select(user => user.Id));
        Assert.Equal([docs.Id], (await GetSharedWithMeAsync(bob)).Select(entry => entry.Id));
    }

    [Fact]
    public async Task KeepsTheOwnerOutOfTheRecipientsWhenTheOwnerJoins()
    {
        ItemEntry file = await Client.UploadEntryAsync(ItemsApi.Root, "report.txt", "a");
        string token = await ShareLinkAsync(file.Id);

        ItemEntry joined = await JoinAsync(Client, token);

        Assert.Equal(file.Id, joined.Id);
        Assert.Equal(file.ParentId, joined.ParentId);
        Assert.True(joined.Permissions.CanModify);
        Assert.Empty(await GetRecipientsAsync(file.Id));
    }

    [Fact]
    public async Task RefusesToJoinWithoutASessionOrAValidToken()
    {
        ItemEntry file = await Client.UploadEntryAsync(ItemsApi.Root, "report.txt", "a");
        string token = await ShareLinkAsync(file.Id);
        using HttpClient anonymous = Factory.CreateClient();

        HttpResponseMessage unauthorized = await anonymous.PostAsync($"/api/shares/{token}/join", null, CancellationToken);
        HttpResponseMessage unknown = await Client.PostAsync("/api/shares/AAAAAAAAAAAAAAAAAAAAAA/join", null, CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, unauthorized.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, unknown.StatusCode);
    }

    private static async Task<ItemEntry> JoinAsync(HttpClient client, string token)
    {
        HttpResponseMessage response = await client.PostAsync($"/api/shares/{token}/join", null, CancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<ItemEntry>(HttpClientExtensions.JsonOptions, CancellationToken)
            ?? throw new InvalidOperationException("The shared item is missing.");
    }

    private async Task<string> ShareLinkAsync(string id)
    {
        HttpResponseMessage response = await Client.PutAsync($"/api/items/{id}/share", null, CancellationToken);
        response.EnsureSuccessStatusCode();
        ShareLink link = await response.Content.ReadFromJsonAsync<ShareLink>(HttpClientExtensions.JsonOptions, CancellationToken)
            ?? throw new InvalidOperationException("The share link is missing.");
        return link.Url[(link.Url.LastIndexOf('/') + 1)..];
    }

    private static async Task<IReadOnlyList<ItemEntry>> GetSharedWithMeAsync(HttpClient client)
    {
        return await client.GetFromJsonAsync<IReadOnlyList<ItemEntry>>("/api/items/shared", HttpClientExtensions.JsonOptions, CancellationToken)
            ?? throw new InvalidOperationException("The shared items are missing.");
    }

    private async Task<IReadOnlyList<UserSummary>> AddRecipientAsync(string id, string userId)
    {
        HttpResponseMessage response = await Client.PutAsync($"/api/items/{id}/recipients/{userId}", null, CancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<IReadOnlyList<UserSummary>>(HttpClientExtensions.JsonOptions, CancellationToken)
            ?? throw new InvalidOperationException("The recipients are missing.");
    }

    private async Task<IReadOnlyList<UserSummary>> GetRecipientsAsync(string id)
    {
        return await Client.GetFromJsonAsync<IReadOnlyList<UserSummary>>($"/api/items/{id}/recipients", HttpClientExtensions.JsonOptions, CancellationToken)
            ?? throw new InvalidOperationException("The recipients are missing.");
    }
}
