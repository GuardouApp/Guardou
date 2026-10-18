using Guardou.Domain.Tasks;

namespace Guardou.Application.Tasks;

public sealed class TaskRunner
{
    private readonly ITaskStore _store;
    private readonly Dictionary<TaskKind, ITaskHandler> _handlers;
    private readonly TimeProvider _time;

    internal TaskRunner(ITaskStore store, IEnumerable<ITaskHandler> handlers, TimeProvider time)
    {
        _store = store;
        _handlers = handlers.ToDictionary(handler => handler.Kind);
        _time = time;
    }

    public async Task<ClaimedTask?> ClaimNextAsync(IReadOnlyCollection<TaskKind> kinds, CancellationToken cancellationToken)
    {
        while (await _store.NextPendingAsync(kinds, cancellationToken) is { } task)
        {
            if (await _store.TryStartAsync(task.Id, _time.GetUtcNow(), cancellationToken))
            {
                return task;
            }
        }

        return null;
    }

    public async Task RunAsync(Guid id, CancellationToken cancellationToken)
    {
        if (await _store.FindAsync(id, cancellationToken) is not { } task)
        {
            return;
        }

        try
        {
            await _handlers[task.Kind].ExecuteAsync(task, cancellationToken);
            _store.Remove(task);
        }
        catch (Exception exception) when (!cancellationToken.IsCancellationRequested)
        {
            task.Fail(exception.Message, _time.GetUtcNow());
        }

        await _store.SaveAsync(CancellationToken.None);
    }

    public Task RecoverAsync(CancellationToken cancellationToken)
    {
        return _store.ResetAsync(null, TaskState.Running, _time.GetUtcNow(), cancellationToken);
    }
}
