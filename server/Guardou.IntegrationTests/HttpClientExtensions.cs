using System.Text.Json;
using System.Text.Json.Serialization;
using System.Net.Http.Json;
using Guardou.Api.Models;

namespace Guardou.IntegrationTests;

internal static class HttpClientExtensions
{
    private const string AntiforgeryHeaderName = "X-XSRF-TOKEN";

    public static JsonSerializerOptions JsonOptions { get; } = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
    };

    public static async Task RefreshAntiforgeryTokenAsync(this HttpClient client)
    {
        AntiforgeryToken? token = await client.GetFromJsonAsync<AntiforgeryToken>("/api/auth/antiforgery", TestContext.Current.CancellationToken);
        client.DefaultRequestHeaders.Remove(AntiforgeryHeaderName);
        client.DefaultRequestHeaders.Add(AntiforgeryHeaderName, token?.Token);
    }

    public static async Task<HttpResponseMessage> LoginAsync(this HttpClient client, string username, string password)
    {
        await client.RefreshAntiforgeryTokenAsync();
        HttpResponseMessage response = await client.PostAsJsonAsync(
            "/api/auth/login",
            new LoginRequest(username, password, RememberMe: false),
            TestContext.Current.CancellationToken);
        await client.RefreshAntiforgeryTokenAsync();
        return response;
    }

    public static async Task<HttpResponseMessage> RegisterAsync(
        this HttpClient client,
        string username,
        string displayName,
        string email,
        string password)
    {
        await client.RefreshAntiforgeryTokenAsync();
        HttpResponseMessage response = await client.PostAsJsonAsync(
            "/api/auth/register",
            new RegisterRequest(username, displayName, email, password),
            TestContext.Current.CancellationToken);
        await client.RefreshAntiforgeryTokenAsync();
        return response;
    }

    public static async Task EnsureAdministratorAsync(this TestServerFactory factory)
    {
        await factory.CreateClient().RegisterAsync(TestServerFactory.AdminUsername, TestServerFactory.AdminDisplayName, TestServerFactory.AdminEmail, TestServerFactory.AdminPassword);
    }
}
