using Guardou.Application.Tasks;
using Guardou.Domain.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Guardou.Infrastructure.Persistence;

internal sealed class EfTaskQueueSettingsStore(AppDbContext context) : ITaskQueueSettingsStore
{
    private DbSet<TaskQueueSettings> Settings => context.Set<TaskQueueSettings>();

    public Task<TaskQueueSettings?> FindAsync(TaskKind kind, CancellationToken cancellationToken)
    {
        return Settings.SingleOrDefaultAsync(settings => settings.Kind == kind, cancellationToken);
    }

    public void Add(TaskQueueSettings settings)
    {
        Settings.Add(settings);
    }

    public Task SaveAsync(CancellationToken cancellationToken)
    {
        return context.SaveChangesAsync(cancellationToken);
    }
}
