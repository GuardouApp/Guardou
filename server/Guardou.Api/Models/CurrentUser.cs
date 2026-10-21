namespace Guardou.Api.Models;

public sealed record CurrentUser(string Id, string Username, string DisplayName, string Email, bool IsAdmin, bool HasPicture);
