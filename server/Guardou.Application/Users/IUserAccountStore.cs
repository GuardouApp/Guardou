namespace Guardou.Application.Users;

public interface IUserAccountStore
{
    Task<bool> AnyUserAsync();

    Task<IReadOnlyList<UserAccount>> ListAsync();

    Task<UserAccount?> FindAsync(string id);

    Task<UserAccount?> FindByUsernameAsync(string username);

    Task<UserAccount?> FindByEmailAsync(string email);

    Task<UserAccount?> FindByLoginAsync(string provider, string key);

    Task<UserAccountResult> CreateAsync(UserProfile profile, string password);

    Task<UserAccountResult> CreateExternalAsync(UserProfile profile, string provider, string key);

    Task<UserAccountResult> AddLoginAsync(string id, string provider, string key);

    Task RemoveLoginAsync(string id, string provider);

    Task<UserAccountResult> UpdateAsync(string id, UserProfile profile, string? password);

    Task DeleteAsync(string id);
}
