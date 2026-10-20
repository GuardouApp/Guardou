namespace Guardou.Application.Authentication;

public interface ISessionService
{
    Task<bool> SignInAsync(string username, string password, bool persistent);

    Task<ExternalLogin?> ReadExternalLoginAsync();

    Task SignInExternalAsync(string userId, ExternalLogin login);

    Task<string?> SignOutAsync(CancellationToken cancellationToken);

    Task RefreshAsync(string userId);
}
