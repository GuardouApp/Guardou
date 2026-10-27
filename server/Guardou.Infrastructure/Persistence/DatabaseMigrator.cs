using Guardou.Application.Persistence;
using Guardou.Infrastructure.Indexing;
using Guardou.Infrastructure.Storage;
using Microsoft.EntityFrameworkCore;

namespace Guardou.Infrastructure.Persistence;

internal sealed class DatabaseMigrator(AppDbContext context, IndexDbContext index, ItemIndexWriter indexWriter, StoragePaths paths)
    : IDatabaseMigrator
{
    private const string WriteAheadLog = "PRAGMA journal_mode=WAL;";

    public async Task MigrateAsync(CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(paths.DatabaseDirectory);
        await MigrateAsync(context, cancellationToken);
        await MigrateAsync(index, cancellationToken);
        await indexWriter.RebuildIfEmptyAsync(cancellationToken);
    }

    private static async Task MigrateAsync(DbContext database, CancellationToken cancellationToken)
    {
        await database.Database.MigrateAsync(cancellationToken);
        await database.Database.ExecuteSqlRawAsync(WriteAheadLog, cancellationToken);
    }
}
