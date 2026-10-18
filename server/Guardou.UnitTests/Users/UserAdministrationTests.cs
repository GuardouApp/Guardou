using Guardou.Application.Users;

namespace Guardou.UnitTests.Users;

public sealed class UserAdministrationTests
{
    private readonly FakeUserAccountStore _store = new();

    [Fact]
    public async Task SetCreatesAMissingUser()
    {
        UserAccountResult result = await new UserAdministration(_store)
            .SetAsync("bob", "bob@example.com", "password", administrator: false);

        UserAccount bob = _store.ByUsername("bob");
        Assert.True(result.Succeeded);
        Assert.Equal("password", _store.Passwords[bob.Id]);
        Assert.Equal("bob@example.com", bob.Email);
        Assert.False(bob.IsAdmin);
    }

    [Fact]
    public async Task SetRequiresAnEmailToCreateAUser()
    {
        UserAccountResult result = await new UserAdministration(_store).SetAsync("bob", null, "password", administrator: false);

        Assert.Equal(UserAccountErrorCodes.EmailRequired, Assert.Single(result.Errors).Code);
        Assert.Empty(_store.Accounts);
    }

    [Fact]
    public async Task SetChangesThePasswordOfAnExistingUser()
    {
        await _store.CreateAsync(new UserProfile("bob", "bob@example.com", IsAdmin: false), "old");

        await new UserAdministration(_store).SetAsync("bob", null, "new", administrator: false);

        UserAccount bob = _store.ByUsername("bob");
        Assert.Equal("new", _store.Passwords[bob.Id]);
        Assert.Equal("bob@example.com", bob.Email);
    }

    [Fact]
    public async Task SetChangesTheEmailOfAnExistingUserWhenGiven()
    {
        await _store.CreateAsync(new UserProfile("bob", "bob@example.com", IsAdmin: false), "old");

        await new UserAdministration(_store).SetAsync("bob", "robert@example.com", "new", administrator: false);

        Assert.Equal("robert@example.com", _store.ByUsername("bob").Email);
    }

    [Fact]
    public async Task SetKeepsTheAdminRoleOfAnExistingAdmin()
    {
        await _store.CreateAsync(new UserProfile("bob", "bob@example.com", IsAdmin: true), "old");

        await new UserAdministration(_store).SetAsync("bob", null, "new", administrator: false);

        Assert.True(_store.ByUsername("bob").IsAdmin);
    }

    [Fact]
    public async Task SetPromotesWhenAdministratorIsRequested()
    {
        await new UserAdministration(_store).SetAsync("bob", "bob@example.com", "password", administrator: true);

        Assert.True(_store.ByUsername("bob").IsAdmin);
    }

    [Fact]
    public async Task PromoteGivesTheAdminRole()
    {
        await _store.CreateAsync(new UserProfile("bob", "bob@example.com", IsAdmin: false), "password");

        UserAccountResult result = await new UserAdministration(_store).PromoteAsync("bob");

        Assert.True(result.Succeeded);
        Assert.True(_store.ByUsername("bob").IsAdmin);
        Assert.Equal("password", _store.Passwords[_store.ByUsername("bob").Id]);
    }

    [Fact]
    public async Task PromoteFailsForAMissingUser()
    {
        UserAccountResult result = await new UserAdministration(_store).PromoteAsync("ghost");

        Assert.Equal(UserAccountErrorCodes.UserNotFound, Assert.Single(result.Errors).Code);
    }
}
