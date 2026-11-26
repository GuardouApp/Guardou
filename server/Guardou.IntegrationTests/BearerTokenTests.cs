using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Guardou.Api.Models;
using Guardou.Application.Users;
using Microsoft.AspNetCore.Authentication.BearerToken;

namespace Guardou.IntegrationTests;

public sealed class BearerTokenTests : SignedInTests
{
    [Fact]
    public async Task TokenAuthenticatesWithoutAntiforgery()
    {
        AccessTokenResponse tokens = await CreateTokenAsync(BobUsername, BobPassword);
        HttpClient client = BearerClient(tokens.AccessToken);

        CurrentUser? me = await client.GetFromJsonAsync<CurrentUser>("/api/auth/me", CancellationToken);
        HttpResponseMessage folder = await client.CreateFolderAsync(ItemsApi.Root, "From token");

        Assert.Equal("Bearer", tokens.TokenType);
        Assert.Equal(BobUsername, me?.Username);
        Assert.Equal(HttpStatusCode.Created, folder.StatusCode);
    }

    [Fact]
    public async Task TokenRefusesAWrongPassword()
    {
        HttpResponseMessage response = await Factory.CreateClient().PostAsJsonAsync(
            "/api/auth/token",
            new TokenRequest(BobUsername, "wrong-password"),
            CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task RefreshGivesANewToken()
    {
        AccessTokenResponse tokens = await CreateTokenAsync(BobUsername, BobPassword);

        HttpResponseMessage response = await RefreshAsync(tokens.RefreshToken);
        AccessTokenResponse refreshed = await ReadTokensAsync(response);
        HttpResponseMessage me = await BearerClient(refreshed.AccessToken).GetAsync("/api/auth/me", CancellationToken);

        Assert.Equal(HttpStatusCode.OK, me.StatusCode);
    }

    [Fact]
    public async Task RefreshRefusesAnInvalidToken()
    {
        HttpResponseMessage response = await RefreshAsync("not-a-token");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task APasswordChangeInvalidatesTheTokens()
    {
        AccessTokenResponse tokens = await CreateTokenAsync(BobUsername, BobPassword);
        UserAccount account = await Client.FindUserAsync(BobUsername);
        await Client.UpdateUserAsync(account.Id, new UserRequest(BobUsername, BobDisplayName, account.Email, "Changed-pass1", IsAdmin: false));

        HttpResponseMessage me = await BearerClient(tokens.AccessToken).GetAsync("/api/auth/me", CancellationToken);
        HttpResponseMessage refresh = await RefreshAsync(tokens.RefreshToken);

        Assert.Equal(HttpStatusCode.Unauthorized, me.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, refresh.StatusCode);
    }

    [Fact]
    public async Task TokenSessionsObeyTheRequiredPasswordChange()
    {
        UserAccount account = await Client.FindUserAsync(BobUsername);
        await Client.UpdateUserAsync(account.Id, new UserRequest(BobUsername, BobDisplayName, account.Email, Password: null, IsAdmin: false, PasswordChangeRequired: true));
        HttpClient client = BearerClient((await CreateTokenAsync(BobUsername, BobPassword)).AccessToken);

        HttpResponseMessage root = await client.GetAsync("/api/items/root", CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, root.StatusCode);
        Assert.Equal(["passwordChangeRequired"], await UsersApi.ReadErrorCodesAsync(root));
    }

    private HttpClient BearerClient(string accessToken)
    {
        HttpClient client = Factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        return client;
    }

    private async Task<AccessTokenResponse> CreateTokenAsync(string username, string password)
    {
        HttpResponseMessage response = await Factory.CreateClient().PostAsJsonAsync(
            "/api/auth/token",
            new TokenRequest(username, password),
            CancellationToken);
        return await ReadTokensAsync(response);
    }

    private Task<HttpResponseMessage> RefreshAsync(string refreshToken)
    {
        return Factory.CreateClient().PostAsJsonAsync("/api/auth/refresh", new RefreshRequest(refreshToken), CancellationToken);
    }

    private static async Task<AccessTokenResponse> ReadTokensAsync(HttpResponseMessage response)
    {
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<AccessTokenResponse>(CancellationToken)
            ?? throw new InvalidOperationException("The token response is missing.");
    }
}
