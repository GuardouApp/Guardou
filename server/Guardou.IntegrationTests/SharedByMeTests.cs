using Guardou.Application.Files;
using Guardou.Application.Users;

namespace Guardou.IntegrationTests;

public sealed class SharedByMeTests : SignedInTests
{
    [Fact]
    public async Task MarksItemsWithALinkOrARecipient()
    {
        ItemEntry docs = await Client.CreateFolderEntryAsync(ItemsApi.Root, "docs");
        ItemEntry report = await Client.UploadEntryAsync(docs.Id, "report.txt", "1");
        ItemEntry notes = await Client.UploadEntryAsync(ItemsApi.Root, "notes.txt", "2");
        await Client.UploadEntryAsync(ItemsApi.Root, "private.txt", "3");
        UserAccount bob = await Client.FindUserAsync(BobUsername);

        await Client.PutAsync($"/api/items/{docs.Id}/share", null, CancellationToken);
        await Client.PutAsync($"/api/items/{notes.Id}/recipients/{bob.Id}", null, CancellationToken);
        ItemDetails root = await Client.GetItemAsync(ItemsApi.Root);

        Assert.False(docs.SharedByMe);
        Assert.Equal(["docs", "notes.txt"], SharedNames(root));
        Assert.False((await Client.GetItemAsync(report.Id)).Entry.SharedByMe);
    }

    [Fact]
    public async Task ClearsTheMarkWhenSharingStops()
    {
        ItemEntry notes = await Client.UploadEntryAsync(ItemsApi.Root, "notes.txt", "1");
        await Client.PutAsync($"/api/items/{notes.Id}/share", null, CancellationToken);

        await Client.DeleteAsync($"/api/items/{notes.Id}/share", CancellationToken);

        Assert.False((await Client.GetItemAsync(notes.Id)).Entry.SharedByMe);
    }

    [Fact]
    public async Task HidesTheMarkFromRecipients()
    {
        ItemEntry notes = await Client.UploadEntryAsync(ItemsApi.Root, "notes.txt", "1");
        UserAccount bob = await Client.FindUserAsync(BobUsername);
        await Client.PutAsync($"/api/items/{notes.Id}/recipients/{bob.Id}", null, CancellationToken);
        using HttpClient bobClient = await SignInAsBobAsync();

        ItemDetails details = await bobClient.GetItemAsync(notes.Id);

        Assert.True((await Client.GetItemAsync(notes.Id)).Entry.SharedByMe);
        Assert.False(details.Entry.SharedByMe);
    }

    private static IEnumerable<string> SharedNames(ItemDetails details)
    {
        return details.Items?.Where(entry => entry.SharedByMe).Select(entry => entry.Name) ?? [];
    }
}
