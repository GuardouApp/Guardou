using Guardou.Domain.Tasks;

namespace Guardou.Application.Tasks;

public interface ITaskHandler
{
    TaskKind Kind { get; }

    Task ExecuteAsync(QueuedTask task, CancellationToken cancellationToken);
}
