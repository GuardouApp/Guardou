using Guardou.Application.Persistence;
using Guardou.Domain.Files;
using Guardou.Infrastructure.Storage;
using Microsoft.EntityFrameworkCore;

namespace Guardou.Infrastructure.Persistence;

internal sealed class DatabaseMigrator(AppDbContext context, DataDirectory dataDirectory, TimeProvider time) : IDatabaseMigrator
{
    public async Task MigrateAsync(CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(dataDirectory.DatabaseDirectory);
        await context.Database.MigrateAsync(cancellationToken);
        await context.Database.ExecuteSqlRawAsync("PRAGMA journal_mode=WAL;", cancellationToken);
        await AddMissingRootFoldersAsync(cancellationToken);
    }

    private async Task AddMissingRootFoldersAsync(CancellationToken cancellationToken)
    {
        IQueryable<string> owners = context.Set<Item>().Where(item => item.ParentId == null).Select(item => item.OwnerId);
        List<string> userIds = await context.Users
            .Where(user => !owners.Contains(user.Id))
            .Select(user => user.Id)
            .ToListAsync(cancellationToken);
        DateTimeOffset now = time.GetUtcNow();
        context.Set<Item>().AddRange(userIds.Select(userId => Item.CreateRoot(userId, now)));
        await context.SaveChangesAsync(cancellationToken);
    }
}
