using System.Net.Http.Json;
using Guardou.Application.Users;

namespace Guardou.IntegrationTests;

public sealed class UserSearchTests : SignedInTests
{
    [Theory]
    [InlineData("bo")]
    [InlineData("BOB@")]
    [InlineData("jones")]
    public async Task SearchesOtherUsers(string query)
    {
        IReadOnlyList<UserSummary> users = await SearchUsersAsync(Client, query);

        Assert.Equal([BobUsername], users.Select(user => user.Username));
    }

    [Theory]
    [InlineData("")]
    [InlineData("%")]
    [InlineData("_")]
    public async Task FindsNoUserForAnEmptyOrWildcardQuery(string query)
    {
        Assert.Empty(await SearchUsersAsync(Client, query));
    }

    [Fact]
    public async Task ExcludesTheCurrentUserFromTheSearch()
    {
        using HttpClient bob = await SignInAsBobAsync();

        Assert.Empty(await SearchUsersAsync(bob, BobUsername));
    }

    private static async Task<IReadOnlyList<UserSummary>> SearchUsersAsync(HttpClient client, string query)
    {
        return await client.GetFromJsonAsync<IReadOnlyList<UserSummary>>(
                $"/api/users/search?query={Uri.EscapeDataString(query)}",
                HttpClientExtensions.JsonOptions,
                CancellationToken)
            ?? throw new InvalidOperationException("The users are missing.");
    }
}
