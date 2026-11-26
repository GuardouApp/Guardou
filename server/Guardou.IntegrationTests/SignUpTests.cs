using System.Net;
using System.Net.Http.Json;
using Guardou.Api.Models;
using Guardou.Application.Configuration;
using Guardou.Domain.Settings;

namespace Guardou.IntegrationTests;

public sealed class SignUpTests : SignedInTests
{
    private static readonly RegisterRequest Carol = new("carol", "Carol", "carol@example.com", "Carol-pass1");

    [Fact]
    public async Task SignUpIsClosedByDefault()
    {
        HttpClient client = Factory.CreateClient();
        await client.RefreshAntiforgeryTokenAsync();

        HttpResponseMessage response = await client.PostAsJsonAsync("/api/auth/signup", Carol, CancellationToken);
        RuntimeConfiguration configuration = await ConfigurationAsync(client);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.False(configuration.SignupOpen);
    }

    [Fact]
    public async Task OpenSignUpCreatesAUserAndSignsIn()
    {
        await Client.PutAsJsonAsync("/api/settings/server", new ServerSettings(null, Signup: true), HttpClientExtensions.JsonOptions, CancellationToken);
        HttpClient client = Factory.CreateClient();
        await client.RefreshAntiforgeryTokenAsync();

        RuntimeConfiguration configuration = await ConfigurationAsync(client);
        HttpResponseMessage response = await client.PostAsJsonAsync("/api/auth/signup", Carol, CancellationToken);
        await client.RefreshAntiforgeryTokenAsync();
        CurrentUser? me = await client.GetFromJsonAsync<CurrentUser>("/api/auth/me", CancellationToken);
        HttpResponseMessage root = await client.GetAsync("/api/items/root", CancellationToken);

        Assert.True(configuration.SignupOpen);
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Equal("carol", me?.Username);
        Assert.False(me?.IsAdmin);
        Assert.Equal(HttpStatusCode.OK, root.StatusCode);
    }

    [Fact]
    public async Task SignUpAppliesThePasswordPolicy()
    {
        await Client.PutAsJsonAsync("/api/settings/server", new ServerSettings(null, Signup: true), HttpClientExtensions.JsonOptions, CancellationToken);
        HttpClient client = Factory.CreateClient();
        await client.RefreshAntiforgeryTokenAsync();

        HttpResponseMessage response = await client.PostAsJsonAsync("/api/auth/signup", Carol with { Password = "short" }, CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    private static async Task<RuntimeConfiguration> ConfigurationAsync(HttpClient client)
    {
        return await client.GetFromJsonAsync<RuntimeConfiguration>("/api/config", HttpClientExtensions.JsonOptions, CancellationToken)
            ?? throw new InvalidOperationException("The configuration is missing.");
    }
}
