using System.Net;
using System.Net.Http.Json;
using Guardou.Api.Models;

namespace Guardou.IntegrationTests;

public sealed class AuthenticationRateLimitTests : SignedInTests
{
    [Fact]
    public async Task TooManyLoginsGiveTooManyRequests()
    {
        HttpClient client = Factory.CreateClient();
        HttpResponseMessage last = new();
        for (int attempt = 0; attempt < 4; attempt++)
        {
            last = await client.LoginAsync(BobUsername, BobPassword);
        }

        Assert.Equal(HttpStatusCode.TooManyRequests, last.StatusCode);
        Assert.NotNull(last.Headers.RetryAfter);
        Assert.Equal(["rateLimited"], await UsersApi.ReadErrorCodesAsync(last));
    }

    [Fact]
    public async Task TheLimitDoesNotApplyToOtherEndpoints()
    {
        for (int attempt = 0; attempt < 6; attempt++)
        {
            await Client.GetAsync("/api/auth/me", CancellationToken);
        }

        CurrentUser? me = await Client.GetFromJsonAsync<CurrentUser>("/api/auth/me", CancellationToken);

        Assert.Equal(TestServerFactory.AdminUsername, me?.Username);
    }

    private protected override TestServerFactory CreateFactory()
    {
        return new TestServerFactory
        {
            Settings = new Dictionary<string, string>
            {
                [TestServerFactory.RateLimitKey] = "4",
            },
        };
    }
}
