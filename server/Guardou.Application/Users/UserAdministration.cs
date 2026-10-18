namespace Guardou.Application.Users;

public sealed class UserAdministration(IUserAccountStore store)
{
    public async Task<UserAccountResult> SetAsync(string username, string? email, string password, bool administrator)
    {
        UserAccount? account = await store.FindByUsernameAsync(username);
        if (account is not null)
        {
            UserProfile profile = new(username, email ?? account.Email, account.IsAdmin || administrator);
            return await store.UpdateAsync(account.Id, profile, password);
        }

        return email is null
            ? UserAccountResult.Failure(UserAccountErrorCodes.EmailRequired, "An email is required to create a user.")
            : await store.CreateAsync(new UserProfile(username, email, administrator), password);
    }

    public async Task<UserAccountResult> PromoteAsync(string username)
    {
        UserAccount? account = await store.FindByUsernameAsync(username);
        return account is null
            ? UserAccountResult.Failure(UserAccountErrorCodes.UserNotFound, $"User '{username}' does not exist.")
            : await store.UpdateAsync(account.Id, account.Profile with { IsAdmin = true }, password: null);
    }
}
