namespace Guardou.Application.Users;

public interface IUserAccountStore
{
    Task<bool> AnyUserAsync();

    Task<IReadOnlyList<UserAccount>> ListAsync();

    Task<UserAccount?> FindAsync(string id);

    Task<UserAccount?> FindByUsernameAsync(string username);

    Task<UserAccountResult> CreateAsync(UserProfile profile, string password);

    Task<UserAccountResult> UpdateAsync(string id, UserProfile profile, string? password);

    Task DeleteAsync(string id);
}
