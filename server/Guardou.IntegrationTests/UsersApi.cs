using System.Net.Http.Json;
using System.Text.Json;
using Guardou.Api.Models;
using Guardou.Application.Users;

namespace Guardou.IntegrationTests;

internal static class UsersApi
{
    private static CancellationToken CancellationToken => TestContext.Current.CancellationToken;

    public static async Task<IReadOnlyList<UserAccount>> ListUsersAsync(this HttpClient client)
    {
        return await client.GetFromJsonAsync<IReadOnlyList<UserAccount>>("/api/users", HttpClientExtensions.JsonOptions, CancellationToken)
            ?? throw new InvalidOperationException("The user list is missing.");
    }

    public static async Task<UserAccount> FindUserAsync(this HttpClient client, string username)
    {
        return (await client.ListUsersAsync()).Single(account => account.Username == username);
    }

    public static Task<HttpResponseMessage> CreateUserAsync(this HttpClient client, UserRequest request)
    {
        return client.PostAsJsonAsync("/api/users", request, HttpClientExtensions.JsonOptions, CancellationToken);
    }

    public static Task<HttpResponseMessage> UpdateUserAsync(this HttpClient client, string id, UserRequest request)
    {
        return client.PutAsJsonAsync($"/api/users/{id}", request, HttpClientExtensions.JsonOptions, CancellationToken);
    }

    public static Task<HttpResponseMessage> DeleteUserAsync(this HttpClient client, string id)
    {
        return client.DeleteAsync($"/api/users/{id}", CancellationToken);
    }

    public static async Task<UserAccount> ReadAccountAsync(HttpResponseMessage response)
    {
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<UserAccount>(HttpClientExtensions.JsonOptions, CancellationToken)
            ?? throw new InvalidOperationException("The user is missing.");
    }

    public static async Task<IReadOnlyList<string>> ReadErrorCodesAsync(HttpResponseMessage response)
    {
        await using Stream content = await response.Content.ReadAsStreamAsync(CancellationToken);
        using JsonDocument problem = await JsonDocument.ParseAsync(content, cancellationToken: CancellationToken);
        return problem.RootElement.GetProperty("codes").EnumerateArray().Select(code => code.GetString() ?? string.Empty).ToList();
    }
}
