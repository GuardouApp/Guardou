namespace Guardou.Application.Authentication;

public interface ISessionService
{
    Task<bool> SignInAsync(string username, string password, bool persistent);

    Task SignOutAsync();

    Task RefreshAsync(string userId);
}
