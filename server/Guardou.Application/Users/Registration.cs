namespace Guardou.Application.Users;

public sealed class Registration(IUserAccountStore store, UserAdministration administration)
{
    private static readonly SemaphoreSlim Gate = new(1, 1);

    public async Task<bool> IsOpenAsync()
    {
        return !await store.AnyUserAsync();
    }

    public async Task<RegistrationResult> RegisterFirstAdministratorAsync(string username, string email, string password)
    {
        await Gate.WaitAsync();
        try
        {
            if (!await IsOpenAsync())
            {
                return RegistrationResult.Closed;
            }

            UserAccountResult result = await administration.SetAsync(username, email, password, administrator: true);
            return result.Succeeded
                ? RegistrationResult.Registered
                : RegistrationResult.Rejected(result.Errors.Select(error => error.Description).ToList());
        }
        finally
        {
            Gate.Release();
        }
    }
}
