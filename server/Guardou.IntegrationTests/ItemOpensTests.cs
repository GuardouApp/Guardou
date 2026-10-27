using System.Net;
using System.Net.Http.Json;
using Guardou.Application.Files;
using Guardou.Application.Users;

namespace Guardou.IntegrationTests;

public sealed class ItemOpensTests : SignedInTests
{
    private static readonly TimeSpan Precision = TimeSpan.FromMilliseconds(1);

    [Fact]
    public async Task RecordsTheLastOpenOfTheUser()
    {
        ItemEntry file = await Client.UploadEntryAsync(ItemsApi.Root, "report.txt", "0123456789");
        Factory.Time.Advance(TimeSpan.FromHours(1));
        HttpResponseMessage first = await OpenAsync(Client, file.Id);
        Factory.Time.Advance(TimeSpan.FromHours(1));
        DateTimeOffset reopened = Factory.Time.GetUtcNow();
        await OpenAsync(Client, file.Id);

        IReadOnlyList<ItemEntry>? children = await Client.GetFromJsonAsync<IReadOnlyList<ItemEntry>>(
            $"/api/items/{ItemsApi.Root}/children",
            HttpClientExtensions.JsonOptions,
            CancellationToken);

        Assert.Null(file.Opened);
        Assert.Equal(HttpStatusCode.NoContent, first.StatusCode);
        Assert.Equal(reopened, (await Client.GetItemAsync(file.Id)).Entry.Opened ?? default, Precision);
        Assert.Equal(reopened, Assert.Single(children ?? []).Opened ?? default, Precision);
    }

    [Fact]
    public async Task ShowsOnlyTheOpensOfTheCurrentUser()
    {
        ItemEntry file = await Client.UploadEntryAsync(ItemsApi.Root, "report.txt", "0123456789");
        UserAccount bob = await Client.FindUserAsync(BobUsername);
        await Client.PutAsync($"/api/items/{file.Id}/recipients/{bob.Id}", null, CancellationToken);
        using HttpClient bobClient = await SignInAsBobAsync();

        HttpResponseMessage open = await OpenAsync(bobClient, file.Id);

        Assert.Equal(HttpStatusCode.NoContent, open.StatusCode);
        Assert.NotNull((await bobClient.GetItemAsync(file.Id)).Entry.Opened);
        Assert.Null((await Client.GetItemAsync(file.Id)).Entry.Opened);
    }

    [Fact]
    public async Task RefusesAFolder()
    {
        ItemEntry folder = await Client.CreateFolderEntryAsync(ItemsApi.Root, "docs");

        HttpResponseMessage open = await OpenAsync(Client, folder.Id);

        Assert.Equal(HttpStatusCode.BadRequest, open.StatusCode);
    }

    [Fact]
    public async Task RefusesAnItemWithoutAccess()
    {
        ItemEntry file = await Client.UploadEntryAsync(ItemsApi.Root, "report.txt", "0123456789");
        using HttpClient bob = await SignInAsBobAsync();

        HttpResponseMessage open = await OpenAsync(bob, file.Id);

        Assert.Equal(HttpStatusCode.NotFound, open.StatusCode);
    }

    private static Task<HttpResponseMessage> OpenAsync(HttpClient client, string id)
    {
        return client.PutAsync($"/api/items/{id}/opened", null, CancellationToken);
    }
}
