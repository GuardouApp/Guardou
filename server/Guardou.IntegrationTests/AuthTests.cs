using System.Net;
using System.Net.Http.Json;
using Guardou.Api.Models;

namespace Guardou.IntegrationTests;

public sealed class AuthTests(TestServerFactory factory) : IClassFixture<TestServerFactory>
{
    private static CancellationToken CancellationToken => TestContext.Current.CancellationToken;

    [Fact]
    public async Task MeRequiresASession()
    {
        HttpResponseMessage response = await factory.CreateClient().GetAsync("/api/auth/me", CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task LoginRequiresAnAntiforgeryToken()
    {
        HttpResponseMessage response = await factory.CreateClient().PostAsJsonAsync(
            "/api/auth/login",
            new LoginRequest(TestServerFactory.AdminUsername, TestServerFactory.AdminPassword, RememberMe: false),
            CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task LoginValidatesTheRequest()
    {
        HttpResponseMessage response = await factory.CreateClient().LoginAsync(string.Empty, string.Empty);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task LoginRefusesAWrongPassword()
    {
        await factory.EnsureAdministratorAsync();
        HttpResponseMessage response = await factory.CreateClient().LoginAsync(TestServerFactory.AdminUsername, "wrong-password");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task LoginStartsASessionForTheAdministrator()
    {
        await factory.EnsureAdministratorAsync();
        HttpClient client = factory.CreateClient();

        HttpResponseMessage login = await client.LoginAsync(TestServerFactory.AdminUsername, TestServerFactory.AdminPassword);
        CurrentUser? me = await client.GetFromJsonAsync<CurrentUser>("/api/auth/me", CancellationToken);

        Assert.Equal(HttpStatusCode.NoContent, login.StatusCode);
        Assert.Equal(new CurrentUser(me?.Id ?? string.Empty, TestServerFactory.AdminUsername, TestServerFactory.AdminEmail, IsAdmin: true), me);
        Assert.NotEmpty(me?.Id ?? string.Empty);
    }

    [Fact]
    public async Task LogoutEndsTheSession()
    {
        await factory.EnsureAdministratorAsync();
        HttpClient client = factory.CreateClient();
        await client.LoginAsync(TestServerFactory.AdminUsername, TestServerFactory.AdminPassword);

        HttpResponseMessage logout = await client.PostAsync("/api/auth/logout", content: null, CancellationToken);
        HttpResponseMessage me = await client.GetAsync("/api/auth/me", CancellationToken);

        Assert.Equal(HttpStatusCode.NoContent, logout.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, me.StatusCode);
    }
}
