namespace Guardou.Application.Persistence;

public interface IDatabaseMigrator
{
    Task MigrateAsync(CancellationToken cancellationToken);
}
