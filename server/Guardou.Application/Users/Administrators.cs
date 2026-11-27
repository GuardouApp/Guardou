namespace Guardou.Application.Users;

public sealed class Administrators(IUserAccountStore store)
{
    public async Task<IReadOnlyList<string>> IdsAsync()
    {
        IReadOnlyList<UserAccount> accounts = await store.ListAsync();
        return accounts.Where(account => account.IsAdmin).Select(account => account.Id).ToList();
    }
}
