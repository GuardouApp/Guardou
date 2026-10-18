namespace Guardou.Application.Users;

public sealed record UserAccountResult(IReadOnlyList<UserAccountError> Errors)
{
    public static UserAccountResult Success { get; } = new([]);

    public bool Succeeded => Errors.Count == 0;

    public static UserAccountResult Failure(string code, string description)
    {
        return new UserAccountResult([new UserAccountError(code, description)]);
    }
}
