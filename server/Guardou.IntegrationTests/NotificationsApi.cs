using System.Net.Http.Json;
using Guardou.Application.Notifications;

namespace Guardou.IntegrationTests;

internal static class NotificationsApi
{
    public static async Task<IReadOnlyList<NotificationEntry>> ListNotificationsAsync(this HttpClient client)
    {
        return await client.GetFromJsonAsync<IReadOnlyList<NotificationEntry>>(
            "/api/notifications",
            HttpClientExtensions.JsonOptions,
            TestContext.Current.CancellationToken) ?? [];
    }

    public static async Task AddRecipientAsync(this HttpClient client, string itemId, string userId)
    {
        (await client.PutAsync($"/api/items/{itemId}/recipients/{userId}", null, TestContext.Current.CancellationToken))
            .EnsureSuccessStatusCode();
    }
}
