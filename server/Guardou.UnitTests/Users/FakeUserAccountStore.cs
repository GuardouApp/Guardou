using Guardou.Application.Users;

namespace Guardou.UnitTests.Users;

internal sealed class FakeUserAccountStore : IUserAccountStore
{
    public Dictionary<string, UserAccount> Accounts { get; } = [];

    public Dictionary<string, string> Passwords { get; } = [];

    public Task<bool> AnyUserAsync()
    {
        return Task.FromResult(Accounts.Count > 0);
    }

    public Task<IReadOnlyList<UserAccount>> ListAsync()
    {
        return Task.FromResult<IReadOnlyList<UserAccount>>(Accounts.Values.ToList());
    }

    public Task<UserAccount?> FindAsync(string id)
    {
        return Task.FromResult(Accounts.GetValueOrDefault(id));
    }

    public Task<UserAccount?> FindByUsernameAsync(string username)
    {
        return Task.FromResult(Accounts.Values.SingleOrDefault(account => account.Username == username));
    }

    public Task<UserAccountResult> CreateAsync(UserProfile profile, string password)
    {
        string id = $"id-{profile.Username}";
        Accounts.Add(id, new UserAccount(id, profile.Username, profile.Email, profile.IsAdmin));
        Passwords.Add(id, password);
        return Task.FromResult(UserAccountResult.Success);
    }

    public Task<UserAccountResult> UpdateAsync(string id, UserProfile profile, string? password)
    {
        Accounts[id] = new UserAccount(id, profile.Username, profile.Email, profile.IsAdmin);
        if (password is not null)
        {
            Passwords[id] = password;
        }

        return Task.FromResult(UserAccountResult.Success);
    }

    public Task DeleteAsync(string id)
    {
        Accounts.Remove(id);
        return Task.CompletedTask;
    }

    public UserAccount ByUsername(string username)
    {
        return Accounts.Values.Single(account => account.Username == username);
    }
}
