using Guardou.Application.Files;
using Guardou.Application.Tasks;
using Guardou.Domain.Files;
using Guardou.Domain.Tasks;

namespace Guardou.Application.Previews;

public sealed class PreviewTasks : ITaskProducer
{
    private const int BatchSize = 500;

    private readonly IItemStore _items;
    private readonly ITaskStore _tasks;
    private readonly IPreviewRenderer _renderer;
    private readonly TaskSignal _signal;
    private readonly TimeProvider _time;

    internal PreviewTasks(
        IItemStore items,
        ITaskStore tasks,
        IPreviewRenderer renderer,
        TaskSignal signal,
        TimeProvider time)
    {
        _items = items;
        _tasks = tasks;
        _renderer = renderer;
        _signal = signal;
        _time = time;
    }

    public TaskKind Kind => TaskKind.Preview;

    public async Task EnqueueAllAsync(CancellationToken cancellationToken)
    {
        await _tasks.ResetAsync(Kind, TaskState.Failed, _time.GetUtcNow(), cancellationToken);
        await EnqueueFilesAsync(cancellationToken);
    }

    internal Task EnqueueAsync(IEnumerable<Item> items, TaskPriority priority, CancellationToken cancellationToken)
    {
        return EnqueueAsync(items.Where(NeedsPreview).ToList(), priority, cancellationToken);
    }

    private async Task EnqueueFilesAsync(CancellationToken cancellationToken)
    {
        Guid? after = null;
        IReadOnlyList<Item> files;
        do
        {
            files = await _items.ActiveFilesAsync(after, BatchSize, cancellationToken);
            await EnqueueAsync(files.Where(CanPreview).ToList(), TaskPriority.Low, cancellationToken);
            after = files.Count > 0 ? files[^1].Id : after;
        }
        while (files.Count == BatchSize);
    }

    private async Task EnqueueAsync(List<Item> items, TaskPriority priority, CancellationToken cancellationToken)
    {
        if (items.Count == 0)
        {
            return;
        }

        List<Guid> itemIds = items.Select(item => item.Id).ToList();
        await _tasks.EnqueueAsync(TaskKind.Preview, itemIds, priority, _time.GetUtcNow(), cancellationToken);
        _signal.Notify();
    }

    private bool CanPreview(Item item)
    {
        return !item.IsFolder && _renderer.Supports(item.Name);
    }

    private bool NeedsPreview(Item item)
    {
        return CanPreview(item) && item.PreviewId is null;
    }
}
