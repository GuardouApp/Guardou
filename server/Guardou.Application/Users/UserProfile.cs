namespace Guardou.Application.Users;

public sealed record UserProfile(string Username, string DisplayName, string Email, bool IsAdmin);
