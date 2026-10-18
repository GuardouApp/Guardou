using Guardou.Application.Files;

namespace Guardou.Application.Users;

public sealed class UserAccounts(IUserAccountStore store, IItemStore items, IBlobStore blobs)
{
    public async Task<IReadOnlyList<UserAccount>> ListAsync()
    {
        IReadOnlyList<UserAccount> accounts = await store.ListAsync();
        return accounts.OrderBy(account => account.Username, StringComparer.OrdinalIgnoreCase).ToList();
    }

    public async Task<UserAccount> GetAsync(string id)
    {
        return await store.FindAsync(id) ?? throw new UserNotFoundException(id);
    }

    public async Task<UserAccount> CreateAsync(UserProfile profile, string? password)
    {
        if (password is null)
        {
            throw Rejected(UserAccountErrorCodes.PasswordRequired, "A password is required to create a user.");
        }

        Ensure(await store.CreateAsync(profile, password));
        return await store.FindByUsernameAsync(profile.Username)
            ?? throw new InvalidOperationException($"The user '{profile.Username}' was not created.");
    }

    public async Task<UserAccount> UpdateAsync(string id, UserProfile profile, string? password)
    {
        UserAccount account = await GetAsync(id);
        if (account.IsAdmin && !profile.IsAdmin && await IsLastAdministratorAsync())
        {
            throw Rejected(UserAccountErrorCodes.LastAdministrator, "The last admin cannot become a user.");
        }

        Ensure(await store.UpdateAsync(id, profile, password));
        return await GetAsync(id);
    }

    public async Task DeleteAsync(string actorId, string id, CancellationToken cancellationToken)
    {
        UserAccount account = await GetAsync(id);
        if (account.Id == actorId)
        {
            throw Rejected(UserAccountErrorCodes.SelfDeletion, "You cannot delete your own user.");
        }

        IReadOnlyList<string> keys = await items.StorageKeysAsync(account.Id, cancellationToken);
        await store.DeleteAsync(account.Id);
        foreach (string key in keys)
        {
            blobs.Delete(key);
        }
    }

    private async Task<bool> IsLastAdministratorAsync()
    {
        IReadOnlyList<UserAccount> accounts = await store.ListAsync();
        return accounts.Count(account => account.IsAdmin) == 1;
    }

    private static void Ensure(UserAccountResult result)
    {
        if (!result.Succeeded)
        {
            throw new UserAccountRejectedException(result.Errors);
        }
    }

    private static UserAccountRejectedException Rejected(string code, string description)
    {
        return new UserAccountRejectedException([new UserAccountError(code, description)]);
    }
}
