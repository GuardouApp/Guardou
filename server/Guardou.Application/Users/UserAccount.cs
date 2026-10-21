namespace Guardou.Application.Users;

public sealed record UserAccount(
    string Id,
    string Username,
    string DisplayName,
    string Email,
    bool IsAdmin,
    bool HasPicture,
    IReadOnlyList<string> ExternalLogins)
{
    internal UserProfile Profile => new(Username, DisplayName, Email, IsAdmin);
}
