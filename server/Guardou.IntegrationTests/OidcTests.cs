using System.Net;
using System.Net.Http.Json;
using Guardou.Application.Authentication;
using Guardou.Application.Configuration;
using Guardou.Application.Users;
using Guardou.Domain.Authentication;
using Guardou.Domain.Settings;
using Microsoft.Extensions.DependencyInjection;

namespace Guardou.IntegrationTests;

public sealed class OidcTests : SignedInTests
{
    private static OidcSettingsChange Configured => new(
        Issuer: "https://id.example.com",
        ClientId: "guardou",
        ClientSecret: "secret",
        Scopes: ["openid", "profile", "email"],
        UserClaim: "preferred_username",
        AutoCreateUsers: true,
        PasswordLogin: true);

    [Fact]
    public async Task ReadsTheSettingsWithoutTheSecret()
    {
        await PutOidcAsync(Configured);

        string json = await Client.GetStringAsync("/api/settings/oidc", CancellationToken);
        OidcSettingsView? view = await Client.GetFromJsonAsync<OidcSettingsView>("/api/settings/oidc", HttpClientExtensions.JsonOptions, CancellationToken);

        Assert.DoesNotContain("secret", json, StringComparison.Ordinal);
        Assert.True(view?.ClientSecretSet);
        Assert.Equal("http://localhost/api/auth/oidc/callback", view?.RedirectUri);
    }

    [Fact]
    public async Task UsesTheExternalUrlForTheRedirectUri()
    {
        HttpResponseMessage server = await Client.PutAsJsonAsync("/api/settings/server", new ServerSettings("https://files.example.com/"), HttpClientExtensions.JsonOptions, CancellationToken);

        OidcSettingsView? view = await Client.GetFromJsonAsync<OidcSettingsView>("/api/settings/oidc", HttpClientExtensions.JsonOptions, CancellationToken);

        Assert.Equal(HttpStatusCode.OK, server.StatusCode);
        Assert.Equal("https://files.example.com/api/auth/oidc/callback", view?.RedirectUri);
    }

    [Fact]
    public async Task RefusesInvalidSettings()
    {
        HttpResponseMessage response = await PutOidcAsync(Configured with { ClientId = null });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task RefusesUsersWithoutTheAdminType()
    {
        using HttpClient bob = await SignInAsBobAsync();

        HttpResponseMessage response = await bob.GetAsync("/api/settings/oidc", CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task ShowsTheConfiguredAuthenticationMethods()
    {
        Assert.Equal([AuthenticationMethod.Password], (await ConfigurationAsync()).AuthenticationMethods);

        await PutOidcAsync(Configured with { PasswordLogin = false });

        Assert.Equal([AuthenticationMethod.Oidc], (await ConfigurationAsync()).AuthenticationMethods);
    }

    [Fact]
    public async Task RefusesThePasswordLoginWhenItIsDisabled()
    {
        await PutOidcAsync(Configured with { PasswordLogin = false });

        HttpResponseMessage response = await Factory.CreateClient().LoginAsync(BobUsername, BobPassword);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task RedirectsToTheLoginPageWhenOidcIsNotConfigured()
    {
        using HttpClient client = Factory.CreateClient(new() { AllowAutoRedirect = false });

        HttpResponseMessage response = await client.GetAsync("/api/auth/oidc/login", CancellationToken);

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("/login?oidcError=notConfigured", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task RedirectsToTheLoginPageWithoutAnExternalLogin()
    {
        await PutOidcAsync(Configured);
        using HttpClient client = Factory.CreateClient(new() { AllowAutoRedirect = false });

        HttpResponseMessage response = await client.GetAsync("/api/auth/oidc/complete?returnUrl=%2Ffiles", CancellationToken);

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("/login?oidcError=failed", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task RemovesTheExternalLoginOfAUser()
    {
        UserAccount bob = await Client.FindUserAsync(BobUsername);
        await using (AsyncServiceScope scope = Factory.Services.CreateAsyncScope())
        {
            await scope.ServiceProvider.GetRequiredService<IUserAccountStore>().AddLoginAsync(bob.Id, OidcSettings.Provider, "bob-subject");
        }

        UserAccount linked = await Client.FindUserAsync(BobUsername);
        HttpResponseMessage response = await Client.DeleteAsync($"/api/users/{bob.Id}/logins/{OidcSettings.Provider}", CancellationToken);

        Assert.Equal([OidcSettings.Provider], linked.ExternalLogins);
        Assert.Empty((await UsersApi.ReadAccountAsync(response)).ExternalLogins);
    }

    private Task<HttpResponseMessage> PutOidcAsync(OidcSettingsChange change)
    {
        return Client.PutAsJsonAsync("/api/settings/oidc", change, HttpClientExtensions.JsonOptions, CancellationToken);
    }

    private async Task<RuntimeConfiguration> ConfigurationAsync()
    {
        return await Client.GetFromJsonAsync<RuntimeConfiguration>("/api/config", HttpClientExtensions.JsonOptions, CancellationToken)
            ?? throw new InvalidOperationException("The configuration is missing.");
    }
}
