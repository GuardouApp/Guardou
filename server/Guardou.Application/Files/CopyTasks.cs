using Guardou.Application.Tasks;
using Guardou.Domain.Files;
using Guardou.Domain.Tasks;

namespace Guardou.Application.Files;

internal sealed class CopyTasks(IItemStore items, ITaskStore tasks, TaskSignal signal, TimeProvider time) : ITaskProducer
{
    public TaskKind Kind => TaskKind.Copy;

    public async Task EnqueueAllAsync(CancellationToken cancellationToken)
    {
        await tasks.ResetAsync(Kind, TaskState.Failed, time.GetUtcNow(), cancellationToken);
        await EnqueueAsync(await items.CopyingFileIdsAsync(cancellationToken), cancellationToken);
    }

    public Task EnqueueAsync(IEnumerable<Item> copies, CancellationToken cancellationToken)
    {
        return EnqueueAsync(copies.Where(copy => copy.IsCopying).Select(copy => copy.Id).ToList(), cancellationToken);
    }

    private async Task EnqueueAsync(IReadOnlyCollection<Guid> itemIds, CancellationToken cancellationToken)
    {
        if (itemIds.Count == 0)
        {
            return;
        }

        await tasks.EnqueueAsync(Kind, itemIds, TaskPriority.Normal, time.GetUtcNow(), cancellationToken);
        signal.Notify();
    }
}
