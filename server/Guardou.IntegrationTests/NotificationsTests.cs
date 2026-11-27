using System.Net;
using System.Net.Http.Json;
using Guardou.Api.Models;
using Guardou.Application.Files;
using Guardou.Application.Notifications;
using Guardou.Application.Shares;
using Guardou.Application.Updates;
using Guardou.Application.Users;
using Guardou.Domain;
using Guardou.Domain.Files;
using Guardou.Domain.Notifications;
using Guardou.Domain.Settings;
using Microsoft.Extensions.DependencyInjection;

namespace Guardou.IntegrationTests;

public sealed class NotificationsTests : SignedInTests
{
    private const string NewerVersion = "99.0.0";

    [Fact]
    public async Task ADirectShareNotifiesTheRecipient()
    {
        ItemEntry docs = await Client.CreateFolderEntryAsync(ItemsApi.Root, "docs");
        UserAccount bob = await Client.FindUserAsync(BobUsername);
        using HttpClient bobClient = await SignInAsBobAsync();

        await Client.AddRecipientAsync(docs.Id, bob.Id);
        await Client.AddRecipientAsync(docs.Id, bob.Id);

        NotificationEntry notification = Assert.Single(await bobClient.ListNotificationsAsync());
        Assert.Equal(NotificationKind.ShareReceived, notification.Kind);
        Assert.False(notification.Read);
        Assert.Equal(new NotificationItem(docs.Id, "docs", ItemKind.Folder), notification.Item);
        Assert.Equal(TestServerFactory.AdminUsername, notification.Sender?.Username);
        Assert.Empty(await Client.ListNotificationsAsync());
    }

    [Fact]
    public async Task JoiningALinkDoesNotNotify()
    {
        ItemEntry docs = await Client.CreateFolderEntryAsync(ItemsApi.Root, "docs");
        ShareLink link = await ReadAsync<ShareLink>(await Client.PutAsync($"/api/items/{docs.Id}/share", null, CancellationToken));
        string token = link.Url[(link.Url.LastIndexOf('/') + 1)..];
        using HttpClient bobClient = await SignInAsBobAsync();

        (await bobClient.PostAsync($"/api/shares/{token}/join", null, CancellationToken)).EnsureSuccessStatusCode();

        Assert.Empty(await bobClient.ListNotificationsAsync());
    }

    [Fact]
    public async Task MarkReadAndClearChangeTheHistory()
    {
        ItemEntry file = await Client.UploadEntryAsync(ItemsApi.Root, "a.txt", "a");
        await Client.AddRecipientAsync(file.Id, (await Client.FindUserAsync(BobUsername)).Id);
        using HttpClient bobClient = await SignInAsBobAsync();

        HttpResponseMessage read = await bobClient.PostAsync("/api/notifications/read", null, CancellationToken);
        bool readState = Assert.Single(await bobClient.ListNotificationsAsync()).Read;
        HttpResponseMessage cleared = await bobClient.DeleteAsync("/api/notifications", CancellationToken);

        Assert.Equal(HttpStatusCode.NoContent, read.StatusCode);
        Assert.True(readState);
        Assert.Equal(HttpStatusCode.NoContent, cleared.StatusCode);
        Assert.Empty(await bobClient.ListNotificationsAsync());
    }

    [Fact]
    public async Task UploadsWarnOnceWhenTheQuotaReachesEightyPercent()
    {
        await SetBobQuotaAsync(100);
        using HttpClient bobClient = await SignInAsBobAsync();

        await bobClient.UploadEntryAsync(ItemsApi.Root, "a.txt", new string('a', 70));
        IReadOnlyList<NotificationEntry> below = await bobClient.ListNotificationsAsync();
        await bobClient.UploadEntryAsync(ItemsApi.Root, "b.txt", new string('b', 15));
        await bobClient.UploadEntryAsync(ItemsApi.Root, "c.txt", new string('c', 5));

        Assert.Empty(below);
        Assert.Equal(NotificationKind.QuotaWarning, Assert.Single(await bobClient.ListNotificationsAsync()).Kind);
    }

    [Fact]
    public async Task ALowerQuotaWarnsWhenTheUsageIsAboveEightyPercent()
    {
        using (HttpClient uploader = await SignInAsBobAsync())
        {
            await uploader.UploadEntryAsync(ItemsApi.Root, "a.txt", new string('a', 90));
        }

        await SetBobQuotaAsync(1000);
        await SetBobQuotaAsync(100);
        using HttpClient bobClient = await SignInAsBobAsync();

        Assert.Equal(NotificationKind.QuotaWarning, Assert.Single(await bobClient.ListNotificationsAsync()).Kind);
    }

    [Fact]
    public async Task ANewerReleaseNotifiesTheAdministratorsOnce()
    {
        Factory.Releases.Latest = "v" + NewerVersion;
        using HttpClient bobClient = await SignInAsBobAsync();

        await RunUpdateCheckAsync();
        await RunUpdateCheckAsync();

        NotificationEntry notification = Assert.Single(await Client.ListNotificationsAsync());
        Assert.Equal(NotificationKind.UpdateAvailable, notification.Kind);
        Assert.Equal(NewerVersion, notification.Version);
        Assert.Equal(Product.ReleaseUrl(NewerVersion), notification.Url);
        Assert.Empty(await bobClient.ListNotificationsAsync());
    }

    [Fact]
    public async Task TheCurrentReleaseDoesNotNotify()
    {
        Factory.Releases.Latest = "v" + Product.Version;

        await RunUpdateCheckAsync();

        Assert.Empty(await Client.ListNotificationsAsync());
    }

    [Fact]
    public async Task ADisabledUpdateCheckDoesNotNotify()
    {
        Factory.Releases.Latest = "v" + NewerVersion;
        ServerSettings disabled = new(ExternalUrl: null, UpdateCheck: false);
        (await Client.PutAsJsonAsync("/api/settings/server", disabled, HttpClientExtensions.JsonOptions, CancellationToken))
            .EnsureSuccessStatusCode();

        await RunUpdateCheckAsync();

        Assert.Empty(await Client.ListNotificationsAsync());
    }

    private async Task RunUpdateCheckAsync()
    {
        await using AsyncServiceScope scope = Factory.Services.CreateAsyncScope();
        await scope.ServiceProvider.GetRequiredService<UpdateCheck>().RunAsync(CancellationToken);
    }

    private async Task SetBobQuotaAsync(long? quotaBytes)
    {
        UserAccount bob = await Client.FindUserAsync(BobUsername);
        UserRequest request = new(bob.Username, bob.DisplayName, bob.Email, Password: null, bob.IsAdmin, quotaBytes);
        (await Client.UpdateUserAsync(bob.Id, request)).EnsureSuccessStatusCode();
    }

    private static async Task<T> ReadAsync<T>(HttpResponseMessage response)
    {
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<T>(HttpClientExtensions.JsonOptions, CancellationToken)
            ?? throw new InvalidOperationException("The response is empty.");
    }
}
