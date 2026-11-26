using System.Net;
using Guardou.Application.Authentication;
using Guardou.Application.Users;

namespace Guardou.IntegrationTests;

public sealed class AuthenticationLimitsTests : SignedInTests
{
    [Fact]
    public async Task LockoutFollowsTheFailedLogins()
    {
        HttpClient client = Factory.CreateClient();
        await client.LoginAsync(BobUsername, "wrong-password");
        HttpResponseMessage second = await client.LoginAsync(BobUsername, "wrong-password");
        HttpResponseMessage locked = await client.LoginAsync(BobUsername, BobPassword);
        UserAccount account = await Client.FindUserAsync(BobUsername);

        Assert.Equal(HttpStatusCode.Forbidden, second.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, locked.StatusCode);
        Assert.Equal(["lockedOut"], await UsersApi.ReadErrorCodesAsync(locked));
        Assert.True(account.LockedOut);
    }

    [Fact]
    public async Task UnlockPermitsTheLoginAgain()
    {
        HttpClient client = Factory.CreateClient();
        await client.LoginAsync(BobUsername, "wrong-password");
        await client.LoginAsync(BobUsername, "wrong-password");
        UserAccount account = await Client.FindUserAsync(BobUsername);

        HttpResponseMessage unlock = await Client.PostAsync($"/api/users/{account.Id}/unlock", content: null, CancellationToken);
        UserAccount unlocked = await UsersApi.ReadAccountAsync(unlock);
        HttpResponseMessage login = await client.LoginAsync(BobUsername, BobPassword);

        Assert.False(unlocked.LockedOut);
        Assert.Equal(HttpStatusCode.NoContent, login.StatusCode);
    }

    [Fact]
    public async Task UnlockRequiresAnAdmin()
    {
        HttpClient bob = await SignInAsBobAsync();
        UserAccount account = await Client.FindUserAsync(BobUsername);

        HttpResponseMessage response = await bob.PostAsync($"/api/users/{account.Id}/unlock", content: null, CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    private protected override TestServerFactory CreateFactory()
    {
        return new TestServerFactory
        {
            Settings = new Dictionary<string, string>
            {
                [$"{AuthenticationLimits.Section}:{nameof(AuthenticationLimits.LockoutMaxAttempts)}"] = "2",
            },
        };
    }
}
