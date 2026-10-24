namespace Guardou.Application.Users;

public interface IUserDirectory
{
    Task<IReadOnlyList<UserSummary>> SearchAsync(string query, string excludedId, int limit, CancellationToken cancellationToken);

    Task<IReadOnlyList<UserSummary>> FindAsync(IReadOnlyCollection<string> ids, CancellationToken cancellationToken);
}
