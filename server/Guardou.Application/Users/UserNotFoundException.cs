namespace Guardou.Application.Users;

public sealed class UserNotFoundException(string id) : Exception($"The user '{id}' does not exist.");
