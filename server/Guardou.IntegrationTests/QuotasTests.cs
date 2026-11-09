using System.Net;
using System.Net.Http.Json;
using Guardou.Api.Models;
using Guardou.Application.Files;
using Guardou.Application.Quotas;
using Guardou.Application.Uploads;
using Guardou.Application.Users;

namespace Guardou.IntegrationTests;

public sealed class QuotasTests : SignedInTests
{
    [Fact]
    public async Task UsageTracksUploadsCopiesTrashAndDeletes()
    {
        ItemEntry file = await Client.UploadEntryAsync(ItemsApi.Root, "a.txt", "hello");
        ItemEntry folder = await Client.CreateFolderEntryAsync(ItemsApi.Root, "docs");
        ItemEntry copy = await ItemsApi.ReadEntryAsync(await Client.TransferAsync("copy", file.Id, folder.Id));
        long afterCopy = (await UsageAsync(Client)).UsedBytes;

        (await Client.PostSelectionAsync("/api/items/trash", copy.Id)).EnsureSuccessStatusCode();
        long afterTrash = (await UsageAsync(Client)).UsedBytes;
        (await Client.PostSelectionAsync("/api/trash/delete", copy.Id)).EnsureSuccessStatusCode();

        Assert.Equal(10, afterCopy);
        Assert.Equal(10, afterTrash);
        Assert.Equal(5, (await UsageAsync(Client)).UsedBytes);
    }

    [Fact]
    public async Task AdminSetsTheQuota()
    {
        UserAccount account = await SetBobQuotaAsync(1024);
        HttpClient bob = await SignInAsBobAsync();

        StorageUsage usage = await UsageAsync(bob);

        Assert.Equal(1024, account.QuotaBytes);
        Assert.Equal(1024, usage.QuotaBytes);
        Assert.Null((await UsageAsync(Client)).QuotaBytes);
    }

    [Fact]
    public async Task UploadBeyondTheQuotaGives413()
    {
        await SetBobQuotaAsync(10);
        HttpClient bob = await SignInAsBobAsync();

        HttpResponseMessage response = await bob.StartUploadAsync(ItemsApi.Root, "large.bin", 11);

        Assert.Equal(HttpStatusCode.RequestEntityTooLarge, response.StatusCode);
        Assert.Equal([QuotaExceededException.Code], await UsersApi.ReadErrorCodesAsync(response));
    }

    [Fact]
    public async Task OpenUploadsReserveSpace()
    {
        await SetBobQuotaAsync(10);
        HttpClient bob = await SignInAsBobAsync();
        UploadSession first = await bob.StartUploadSessionAsync(ItemsApi.Root, "first.bin", 6);

        HttpResponseMessage refused = await bob.StartUploadAsync(ItemsApi.Root, "second.bin", 5);
        (await bob.CancelUploadAsync(first.Id)).EnsureSuccessStatusCode();
        HttpResponseMessage accepted = await bob.StartUploadAsync(ItemsApi.Root, "second.bin", 5);

        Assert.Equal(HttpStatusCode.RequestEntityTooLarge, refused.StatusCode);
        Assert.Equal(HttpStatusCode.Created, accepted.StatusCode);
    }

    [Fact]
    public async Task CopyBeyondTheQuotaGives413()
    {
        await SetBobQuotaAsync(8);
        HttpClient bob = await SignInAsBobAsync();
        ItemEntry file = await bob.UploadEntryAsync(ItemsApi.Root, "a.txt", "hello");
        ItemEntry folder = await bob.CreateFolderEntryAsync(ItemsApi.Root, "docs");
        IReadOnlyList<string> blobs = BlobFiles();

        HttpResponseMessage response = await bob.TransferAsync("copy", file.Id, folder.Id);

        Assert.Equal(HttpStatusCode.RequestEntityTooLarge, response.StatusCode);
        Assert.Equal(blobs, BlobFiles());
        Assert.Equal(5, (await UsageAsync(await SignInAsBobAsync())).UsedBytes);
    }

    [Fact]
    public async Task ProfileChangesKeepTheUsage()
    {
        HttpClient bob = await SignInAsBobAsync();
        await bob.UploadEntryAsync(ItemsApi.Root, "a.txt", "hello");

        UserAccount account = await SetBobQuotaAsync(null);

        Assert.Equal(5, (await UsageAsync(await SignInAsBobAsync())).UsedBytes);
        Assert.Null(account.QuotaBytes);
    }

    private async Task<UserAccount> SetBobQuotaAsync(long? quotaBytes)
    {
        UserAccount bob = await Client.FindUserAsync(BobUsername);
        UserRequest request = new(bob.Username, bob.DisplayName, bob.Email, Password: null, bob.IsAdmin, quotaBytes);
        return await UsersApi.ReadAccountAsync(await Client.UpdateUserAsync(bob.Id, request));
    }

    private static async Task<StorageUsage> UsageAsync(HttpClient client)
    {
        return await client.GetFromJsonAsync<StorageUsage>("/api/items/usage", HttpClientExtensions.JsonOptions, CancellationToken)
            ?? throw new InvalidOperationException("The usage is missing.");
    }
}
