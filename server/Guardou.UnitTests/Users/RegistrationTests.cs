using Guardou.Application.Users;

namespace Guardou.UnitTests.Users;

public sealed class RegistrationTests
{
    private readonly FakeUserAccountStore _store = new();

    private Registration CreateRegistration()
    {
        return new Registration(_store, new UserAdministration(_store));
    }

    [Fact]
    public async Task IsOpenWhenNoUserExists()
    {
        Assert.True(await CreateRegistration().IsOpenAsync());
    }

    [Fact]
    public async Task TheFirstUserBecomesAnAdministrator()
    {
        RegistrationResult result = await CreateRegistration().RegisterFirstAdministratorAsync("alice", "Alice", "alice@example.com", "password");

        Assert.Equal(RegistrationStatus.Registered, result.Status);
        Assert.True(_store.ByUsername("alice").IsAdmin);
    }

    [Fact]
    public async Task ClosesAfterTheFirstUser()
    {
        Registration registration = CreateRegistration();
        await registration.RegisterFirstAdministratorAsync("alice", "Alice", "alice@example.com", "password");

        RegistrationResult result = await registration.RegisterFirstAdministratorAsync("bob", "Bob", "bob@example.com", "password");

        Assert.False(await registration.IsOpenAsync());
        Assert.Equal(RegistrationStatus.Closed, result.Status);
        Assert.Null(await _store.FindByUsernameAsync("bob"));
    }
}
