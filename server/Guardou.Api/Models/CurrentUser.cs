namespace Guardou.Api.Models;

public sealed record CurrentUser(string Id, string Username, string Email, bool IsAdmin);
