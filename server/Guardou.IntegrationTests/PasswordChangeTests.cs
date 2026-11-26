using System.Net;
using System.Net.Http.Json;
using Guardou.Api.Models;
using Guardou.Application.Users;

namespace Guardou.IntegrationTests;

public sealed class PasswordChangeTests : SignedInTests
{
    private const string NewPassword = "Changed-pass1";

    [Fact]
    public async Task RequiredChangeBlocksTheOtherEndpoints()
    {
        HttpClient bob = await RequireChangeForBobAsync();

        CurrentUser? me = await bob.GetFromJsonAsync<CurrentUser>("/api/auth/me", CancellationToken);
        HttpResponseMessage root = await bob.GetAsync("/api/items/root", CancellationToken);

        Assert.True(me?.PasswordChangeRequired);
        Assert.Equal(HttpStatusCode.Forbidden, root.StatusCode);
        Assert.Equal(["passwordChangeRequired"], await UsersApi.ReadErrorCodesAsync(root));
    }

    [Fact]
    public async Task ChangeClearsTheRequirementAndKeepsTheSession()
    {
        HttpClient bob = await RequireChangeForBobAsync();

        HttpResponseMessage change = await ChangeAsync(bob, BobPassword, NewPassword);
        CurrentUser? me = await bob.GetFromJsonAsync<CurrentUser>("/api/auth/me", CancellationToken);
        HttpResponseMessage root = await bob.GetAsync("/api/items/root", CancellationToken);
        HttpResponseMessage login = await Factory.CreateClient().LoginAsync(BobUsername, NewPassword);

        Assert.Equal(HttpStatusCode.NoContent, change.StatusCode);
        Assert.False(me?.PasswordChangeRequired);
        Assert.Equal(HttpStatusCode.OK, root.StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, login.StatusCode);
    }

    [Fact]
    public async Task ChangeRefusesAWrongCurrentPassword()
    {
        HttpClient bob = await SignInAsBobAsync();

        HttpResponseMessage change = await ChangeAsync(bob, "wrong-password", NewPassword);

        Assert.Equal(HttpStatusCode.BadRequest, change.StatusCode);
        Assert.Equal(["PasswordMismatch"], await UsersApi.ReadErrorCodesAsync(change));
    }

    [Fact]
    public async Task ChangeAppliesThePasswordPolicy()
    {
        HttpClient bob = await SignInAsBobAsync();

        HttpResponseMessage change = await ChangeAsync(bob, BobPassword, "short");

        Assert.Equal(HttpStatusCode.BadRequest, change.StatusCode);
    }

    [Fact]
    public async Task ChangeRequiresASession()
    {
        HttpClient client = Factory.CreateClient();
        await client.RefreshAntiforgeryTokenAsync();

        HttpResponseMessage change = await ChangeAsync(client, BobPassword, NewPassword);

        Assert.Equal(HttpStatusCode.Unauthorized, change.StatusCode);
    }

    private async Task<HttpClient> RequireChangeForBobAsync()
    {
        UserAccount account = await Client.FindUserAsync(BobUsername);
        UserAccount updated = await UsersApi.ReadAccountAsync(await Client.UpdateUserAsync(
            account.Id,
            new UserRequest(BobUsername, BobDisplayName, account.Email, Password: null, IsAdmin: false, PasswordChangeRequired: true)));
        Assert.True(updated.PasswordChangeRequired);
        return await SignInAsBobAsync();
    }

    private static Task<HttpResponseMessage> ChangeAsync(HttpClient client, string current, string password)
    {
        return client.PutAsJsonAsync("/api/users/me/password", new PasswordChangeRequest(current, password), CancellationToken);
    }
}
