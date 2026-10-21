using System.Net;
using System.Net.Http.Json;
using Guardou.Api.Models;
using Guardou.Application.Configuration;

namespace Guardou.IntegrationTests;

public sealed class RegistrationTests
{
    private static CancellationToken CancellationToken => TestContext.Current.CancellationToken;

    [Fact]
    public async Task TheFirstUserBecomesASignedInAdministrator()
    {
        await using TestServerFactory factory = new();
        HttpClient client = factory.CreateClient();

        HttpResponseMessage response = await client.RegisterAsync("alice", "Alice", "alice@example.com", "First-user-pass1");
        CurrentUser? me = await client.GetFromJsonAsync<CurrentUser>("/api/auth/me", CancellationToken);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Equal(new CurrentUser(me?.Id ?? string.Empty, "alice", "Alice", "alice@example.com", IsAdmin: true, HasPicture: false), me);
        Assert.NotEmpty(me?.Id ?? string.Empty);
    }

    [Fact]
    public async Task RegistrationClosesAfterTheFirstUser()
    {
        await using TestServerFactory factory = new();
        await factory.CreateClient().RegisterAsync("alice", "Alice", "alice@example.com", "First-user-pass1");

        HttpResponseMessage response = await factory.CreateClient().RegisterAsync("bob", "Bob", "bob@example.com", "Second-user-pass1");
        RuntimeConfiguration? configuration = await factory.CreateClient()
            .GetFromJsonAsync<RuntimeConfiguration>("/api/config", HttpClientExtensions.JsonOptions, CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.False(configuration?.RegistrationOpen);
    }

    [Fact]
    public async Task RegistrationRequiresAValidEmail()
    {
        await using TestServerFactory factory = new();

        HttpResponseMessage response = await factory.CreateClient().RegisterAsync("alice", "Alice", "not-an-email", "First-user-pass1");
        RuntimeConfiguration? configuration = await factory.CreateClient()
            .GetFromJsonAsync<RuntimeConfiguration>("/api/config", HttpClientExtensions.JsonOptions, CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.True(configuration?.RegistrationOpen);
    }

    [Fact]
    public async Task RegistrationRefusesAnInvalidUser()
    {
        await using TestServerFactory factory = new();

        HttpResponseMessage response = await factory.CreateClient().RegisterAsync("john.doe", "John.doe", "john@example.com", "short");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }
}
