using System.Net;
using System.Net.Http.Json;
using Guardou.Api.Models;
using Guardou.Application.Authentication;
using Guardou.Domain.Authentication;
using Guardou.Infrastructure.Profiles;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.DependencyInjection;

namespace Guardou.IntegrationTests;

public sealed class OidcFlowTests : IAsyncLifetime
{
    private readonly FakeIdentityProvider _provider = new();
    private readonly TestServerFactory _factory;

    public OidcFlowTests()
    {
        _factory = new TestServerFactory
        {
            TestServices = services =>
            {
                services.Configure<OpenIdConnectOptions>(OidcSettings.Provider, options => options.BackchannelHttpHandler = _provider);
                services.AddHttpClient<ProfilePictureDownloader>()
                    .ConfigurePrimaryHttpMessageHandler(() => _provider)
                    .SetHandlerLifetime(Timeout.InfiniteTimeSpan);
            },
        };
    }

    private static CancellationToken CancellationToken => TestContext.Current.CancellationToken;

    public async ValueTask InitializeAsync()
    {
        await _factory.EnsureAdministratorAsync();
        using HttpClient admin = _factory.CreateClient();
        await admin.LoginAsync(TestServerFactory.AdminUsername, TestServerFactory.AdminPassword);
        OidcSettingsChange settings = new(
            FakeIdentityProvider.Issuer,
            FakeIdentityProvider.ClientId,
            ClientSecret: "secret",
            Scopes: ["openid", "profile", "email"],
            UserClaim: "preferred_username",
            AutoCreateUsers: true,
            PasswordLogin: true);
        HttpResponseMessage response = await admin.PutAsJsonAsync("/api/settings/oidc", settings, HttpClientExtensions.JsonOptions, CancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async ValueTask DisposeAsync()
    {
        await _factory.DisposeAsync();
        _provider.Dispose();
    }

    [Fact]
    public async Task SignsInWithTheProviderAndSignsOutAtTheProvider()
    {
        _provider.Claims["sub"] = "carol-subject";
        _provider.Claims["preferred_username"] = "carol.doe";
        _provider.Claims["email"] = "carol@example.com";
        _provider.Claims["email_verified"] = true;
        _provider.Claims["display_name"] = "Carol Doe";
        _provider.Claims["picture"] = FakeIdentityProvider.PictureUrl;
        using HttpClient client = _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        Uri completion = await AuthorizeAsync(client, "/files");
        HttpResponseMessage completed = await client.GetAsync(completion, CancellationToken);
        CurrentUser? me = await client.GetFromJsonAsync<CurrentUser>("/api/auth/me", CancellationToken);
        HttpResponseMessage picture = await client.GetAsync($"/api/users/{me?.Id}/picture", CancellationToken);

        Assert.Equal("/files", completed.Headers.Location?.OriginalString);
        Assert.Equal(new CurrentUser(me?.Id ?? string.Empty, "caroldoe", "Carol Doe", "carol@example.com", IsAdmin: false, HasPicture: true), me);
        Assert.Equal("image/webp", picture.Content.Headers.ContentType?.MediaType);

        await client.RefreshAntiforgeryTokenAsync();
        HttpResponseMessage logout = await client.PostAsync("/api/auth/logout", content: null, CancellationToken);
        LogoutResult? result = await logout.Content.ReadFromJsonAsync<LogoutResult>(CancellationToken);
        Dictionary<string, Microsoft.Extensions.Primitives.StringValues> query = QueryHelpers.ParseQuery(new Uri(result?.RedirectUrl ?? FakeIdentityProvider.Issuer).Query);

        Assert.StartsWith(FakeIdentityProvider.EndSessionEndpoint, result?.RedirectUrl, StringComparison.Ordinal);
        Assert.Equal("http://localhost/login", query["post_logout_redirect_uri"]);
        Assert.NotEmpty(query["id_token_hint"].ToString());
    }

    [Fact]
    public async Task RedirectsToTheLoginPageWhenTheProviderRefuses()
    {
        using HttpClient client = _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        HttpResponseMessage challenge = await client.GetAsync("/api/auth/oidc/login", CancellationToken);
        string state = QueryHelpers.ParseQuery(challenge.Headers.Location?.Query)["state"].ToString();

        HttpResponseMessage callback = await client.GetAsync($"/api/auth/oidc/callback?error=access_denied&state={Uri.EscapeDataString(state)}", CancellationToken);

        Assert.Equal("/login?oidcError=failed", callback.Headers.Location?.OriginalString);
    }

    private async Task<Uri> AuthorizeAsync(HttpClient client, string returnUrl)
    {
        HttpResponseMessage challenge = await client.GetAsync($"/api/auth/oidc/login?returnUrl={Uri.EscapeDataString(returnUrl)}", CancellationToken);
        Uri authorize = challenge.Headers.Location ?? throw new InvalidOperationException("The challenge has no redirect.");
        Dictionary<string, Microsoft.Extensions.Primitives.StringValues> query = QueryHelpers.ParseQuery(authorize.Query);
        Assert.Equal("http://localhost/api/auth/oidc/callback", query["redirect_uri"]);
        _provider.Nonce = query["nonce"];

        HttpResponseMessage callback = await client.GetAsync($"/api/auth/oidc/callback?code=code&state={Uri.EscapeDataString(query["state"].ToString())}", CancellationToken);
        Assert.Equal(HttpStatusCode.Redirect, callback.StatusCode);
        return callback.Headers.Location ?? throw new InvalidOperationException("The callback has no redirect.");
    }
}
