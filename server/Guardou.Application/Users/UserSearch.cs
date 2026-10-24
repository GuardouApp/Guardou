namespace Guardou.Application.Users;

public sealed class UserSearch(IUserDirectory directory)
{
    private const int Limit = 10;

    public async Task<IReadOnlyList<UserSummary>> SearchAsync(string userId, string? query, CancellationToken cancellationToken)
    {
        return string.IsNullOrWhiteSpace(query) ? [] : await directory.SearchAsync(query.Trim(), userId, Limit, cancellationToken);
    }
}
