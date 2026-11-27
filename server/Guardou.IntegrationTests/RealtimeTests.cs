using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Guardou.Api.Models;
using Guardou.Api.Realtime;
using Guardou.Application.Files;
using Guardou.Application.Notifications;
using Guardou.Domain.Notifications;
using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;

namespace Guardou.IntegrationTests;

public sealed class RealtimeTests : SignedInTests
{
    private static readonly TimeSpan EventTimeout = TimeSpan.FromSeconds(10);

    [Fact]
    public async Task RefusesAnonymousConnections()
    {
        HttpResponseMessage response = await Factory.CreateClient().PostAsync("/api/events/negotiate?negotiateVersion=1", null, CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task AnUploadSendsTheFolderToTheOwner()
    {
        string rootId = (await Client.GetItemAsync(ItemsApi.Root)).Entry.Id;
        await using HubConnection admin = await ConnectAsync(TestServerFactory.AdminUsername, TestServerFactory.AdminPassword);
        Task<ItemsChangedEvent> changed = NextAsync<ItemsChangedEvent>(admin, nameof(IEventsClient.ItemsChanged));

        await Client.UploadEntryAsync(ItemsApi.Root, "a.txt", "a");

        Assert.Contains(rootId, (await changed).FolderIds);
    }

    [Fact]
    public async Task ADirectShareSendsTheNotificationAndTheShareChange()
    {
        ItemEntry file = await Client.UploadEntryAsync(ItemsApi.Root, "a.txt", "a");
        await using HubConnection bob = await ConnectAsync(BobUsername, BobPassword);
        Task<NotificationEntry> notification = NextAsync<NotificationEntry>(bob, nameof(IEventsClient.NotificationReceived));
        TaskCompletionSource shares = new(TaskCreationOptions.RunContinuationsAsynchronously);
        bob.On(nameof(IEventsClient.SharesChanged), shares.SetResult);

        await Client.AddRecipientAsync(file.Id, (await Client.FindUserAsync(BobUsername)).Id);

        NotificationEntry received = await notification.WaitAsync(EventTimeout, CancellationToken);
        await shares.Task.WaitAsync(EventTimeout, CancellationToken);
        Assert.Equal(NotificationKind.ShareReceived, received.Kind);
        Assert.Equal(file.Id, received.Item?.Id);
    }

    private static async Task<T> NextAsync<T>(HubConnection connection, string method)
    {
        TaskCompletionSource<T> received = new(TaskCreationOptions.RunContinuationsAsynchronously);
        using IDisposable subscription = connection.On<T>(method, value => received.TrySetResult(value));
        return await received.Task.WaitAsync(EventTimeout, CancellationToken);
    }

    private async Task<HubConnection> ConnectAsync(string username, string password)
    {
        string token = await CreateTokenAsync(username, password);
        HubConnection connection = new HubConnectionBuilder()
            .WithUrl(new Uri(Factory.Server.BaseAddress, EventsHub.Path), options =>
            {
                options.Transports = HttpTransportType.LongPolling;
                options.HttpMessageHandlerFactory = _ => Factory.Server.CreateHandler();
                options.AccessTokenProvider = () => Task.FromResult<string?>(token);
            })
            .AddJsonProtocol(options => options.PayloadSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)))
            .Build();
        await connection.StartAsync(CancellationToken);
        return connection;
    }

    private async Task<string> CreateTokenAsync(string username, string password)
    {
        HttpResponseMessage response = await Factory.CreateClient().PostAsJsonAsync("/api/auth/token", new TokenRequest(username, password), CancellationToken);
        response.EnsureSuccessStatusCode();
        AccessTokenResponse tokens = await response.Content.ReadFromJsonAsync<AccessTokenResponse>(CancellationToken)
            ?? throw new InvalidOperationException("The token response is empty.");
        return tokens.AccessToken;
    }
}
