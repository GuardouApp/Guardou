using Guardou.Application.Persistence;
using Guardou.Infrastructure.Storage;
using Microsoft.EntityFrameworkCore;

namespace Guardou.Infrastructure.Persistence;

internal sealed class DatabaseMigrator(AppDbContext context, StoragePaths paths) : IDatabaseMigrator
{
    public async Task MigrateAsync(CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(paths.DatabaseDirectory);
        await context.Database.MigrateAsync(cancellationToken);
        await context.Database.ExecuteSqlRawAsync("PRAGMA journal_mode=WAL;", cancellationToken);
    }
}
