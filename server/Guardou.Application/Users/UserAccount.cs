namespace Guardou.Application.Users;

public sealed record UserAccount(
    string Id,
    string Username,
    string Email,
    bool IsAdmin,
    IReadOnlyList<string> ExternalLogins)
{
    internal UserProfile Profile => new(Username, Email, IsAdmin);
}
