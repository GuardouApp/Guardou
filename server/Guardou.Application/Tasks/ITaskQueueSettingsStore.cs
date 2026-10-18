using Guardou.Domain.Tasks;

namespace Guardou.Application.Tasks;

public interface ITaskQueueSettingsStore
{
    Task<TaskQueueSettings?> FindAsync(TaskKind kind, CancellationToken cancellationToken);

    void Add(TaskQueueSettings settings);

    Task SaveAsync(CancellationToken cancellationToken);
}
