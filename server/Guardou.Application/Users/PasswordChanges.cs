namespace Guardou.Application.Users;

public sealed class PasswordChanges(IUserAccountStore store)
{
    public async Task ChangeAsync(string userId, string currentPassword, string newPassword)
    {
        UserAccountResult result = await store.ChangePasswordAsync(userId, currentPassword, newPassword);
        if (!result.Succeeded)
        {
            throw new UserAccountRejectedException(result.Errors);
        }
    }
}
