using Guardou.Domain.Files;
using Guardou.Domain.Tasks;

namespace Guardou.Application.Tasks;

public sealed class TaskAdministration
{
    private readonly ITaskStore _store;
    private readonly TaskRunner _runner;
    private readonly Dictionary<TaskKind, ITaskProducer> _producers;
    private readonly TaskSignal _signal;
    private readonly TimeProvider _time;

    internal TaskAdministration(
        ITaskStore store,
        TaskRunner runner,
        IEnumerable<ITaskProducer> producers,
        TaskSignal signal,
        TimeProvider time)
    {
        _store = store;
        _runner = runner;
        _producers = producers.ToDictionary(producer => producer.Kind);
        _signal = signal;
        _time = time;
    }

    public async Task<IReadOnlyList<TaskQueueSummary>> QueuesAsync(CancellationToken cancellationToken)
    {
        IReadOnlyDictionary<TaskKind, TaskCounts> counts = await _store.CountAsync(cancellationToken);
        return _producers.Keys
            .Order()
            .Select(kind => (Kind: kind, Count: counts.GetValueOrDefault(kind, TaskCounts.None)))
            .Select(queue => new TaskQueueSummary(queue.Kind, queue.Count.Pending, queue.Count.Running, queue.Count.Failed))
            .ToList();
    }

    public Task<IReadOnlyList<TaskEntry>> ListAsync(TaskKind? kind, TaskState? state, int limit, CancellationToken cancellationToken)
    {
        return _store.ListAsync(kind, state, limit, cancellationToken);
    }

    public async Task<TaskEntry?> RunNowAsync(string id, CancellationToken cancellationToken)
    {
        QueuedTask task = await FindAsync(id, cancellationToken);
        if (task.State == TaskState.Failed)
        {
            task.Reset(_time.GetUtcNow());
            await _store.SaveAsync(cancellationToken);
        }

        if (!await _store.TryStartAsync(task.Id, _time.GetUtcNow(), cancellationToken))
        {
            throw new TaskConflictException(id);
        }

        await _runner.RunAsync(task.Id, cancellationToken);
        return await _store.FindEntryAsync(task.Id, cancellationToken);
    }

    public async Task<TaskEntry> ResetAsync(string id, CancellationToken cancellationToken)
    {
        QueuedTask task = await FindAsync(id, cancellationToken);
        if (task.State != TaskState.Failed)
        {
            throw new TaskConflictException(id);
        }

        task.Reset(_time.GetUtcNow());
        await _store.SaveAsync(cancellationToken);
        TaskEntry entry = await _store.FindEntryAsync(task.Id, cancellationToken) ?? throw new TaskNotFoundException(id);
        _signal.Notify();
        return entry;
    }

    public async Task ResetFailedAsync(TaskKind kind, CancellationToken cancellationToken)
    {
        await _store.ResetAsync(kind, TaskState.Failed, _time.GetUtcNow(), cancellationToken);
        _signal.Notify();
    }

    public Task EnqueueAllAsync(TaskKind kind, CancellationToken cancellationToken)
    {
        return ProducerOf(kind).EnqueueAllAsync(cancellationToken);
    }

    private ITaskProducer ProducerOf(TaskKind kind)
    {
        return _producers.TryGetValue(kind, out ITaskProducer? producer) ? producer : throw new TaskNotFoundException(kind.ToString());
    }

    private async Task<QueuedTask> FindAsync(string id, CancellationToken cancellationToken)
    {
        return ItemId.TryParse(id, out Guid taskId) && await _store.FindAsync(taskId, cancellationToken) is { } task
            ? task
            : throw new TaskNotFoundException(id);
    }
}
