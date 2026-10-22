using Guardou.Application.Files;
using Guardou.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Guardou.Infrastructure.Identity;

internal sealed class EfUserNames(AppDbContext context) : IUserNames
{
    public async Task<IReadOnlyDictionary<string, string>> DisplayNamesAsync(
        IReadOnlyCollection<string> ids,
        CancellationToken cancellationToken)
    {
        return await context.Users
            .Where(user => ids.Contains(user.Id))
            .ToDictionaryAsync(user => user.Id, user => user.DisplayName, cancellationToken);
    }
}
