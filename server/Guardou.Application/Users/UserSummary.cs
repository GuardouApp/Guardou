namespace Guardou.Application.Users;

public sealed record UserSummary(string Id, string Username, string DisplayName, string Email, bool HasPicture);
