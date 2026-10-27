using Guardou.Application.Files;
using Guardou.Application.Tasks;
using Guardou.Domain.Tasks;

namespace Guardou.Application.Search;

public sealed class IndexTasks : ITaskProducer
{
    private readonly IItemStore _items;
    private readonly ITaskStore _tasks;
    private readonly TaskSignal _signal;
    private readonly TimeProvider _time;

    internal IndexTasks(IItemStore items, ITaskStore tasks, TaskSignal signal, TimeProvider time)
    {
        _items = items;
        _tasks = tasks;
        _signal = signal;
        _time = time;
    }

    public TaskKind Kind => TaskKind.SearchIndex;

    public async Task EnqueueAllAsync(CancellationToken cancellationToken)
    {
        await _tasks.ResetAsync(Kind, TaskState.Failed, _time.GetUtcNow(), cancellationToken);
        await _tasks.EnqueueAsync(Kind, await _items.RootIdsAsync(cancellationToken), TaskPriority.Low, _time.GetUtcNow(), cancellationToken);
        _signal.Notify();
    }
}
