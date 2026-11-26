namespace Guardou.Application.Authentication;

public interface ISessionService
{
    Task<PasswordLoginStatus> SignInAsync(string username, string password, SessionType type, bool persistent);

    Task<bool> RefreshTokenAsync(string refreshToken);

    Task<ExternalLogin?> ReadExternalLoginAsync();

    Task SignInExternalAsync(string userId, ExternalLogin login);

    Task<string?> SignOutAsync(CancellationToken cancellationToken);

    Task RefreshAsync(string userId);
}
