namespace Guardou.Application.Users;

public sealed class UserAccountRejectedException(IReadOnlyList<UserAccountError> errors)
    : Exception(string.Join(" ", errors.Select(error => error.Description)))
{
    public IReadOnlyList<UserAccountError> Errors { get; } = errors;
}
