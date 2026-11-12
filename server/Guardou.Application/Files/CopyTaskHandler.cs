using Guardou.Application.Previews;
using Guardou.Application.Tasks;
using Guardou.Domain.Files;
using Guardou.Domain.Tasks;

namespace Guardou.Application.Files;

internal sealed class CopyTaskHandler(IItemStore items, IBlobStore blobs, PreviewTasks previews, TrashService trash) : ITaskHandler
{
    public TaskKind Kind => TaskKind.Copy;

    public async Task ExecuteAsync(QueuedTask task, CancellationToken cancellationToken)
    {
        if (await items.FindAnyAsync(task.ItemId, cancellationToken) is not { CopySourceKey: { } sourceKey, StorageKey: { } key } copy)
        {
            return;
        }

        if (!blobs.Exists(sourceKey))
        {
            await TrashAsync(copy, cancellationToken);
            return;
        }

        await blobs.CopyAsync(sourceKey, key, cancellationToken);
        if (!await items.CompleteCopyAsync(copy.Id, cancellationToken))
        {
            blobs.Delete(key);
            return;
        }

        await previews.EnqueueAsync([copy], TaskPriority.Normal, cancellationToken);
    }

    private async Task TrashAsync(Item copy, CancellationToken cancellationToken)
    {
        try
        {
            await trash.MoveToTrashAsync(copy.OwnerId, [ItemId.Format(copy.Id)], cancellationToken);
        }
        catch (ItemNotFoundException)
        {
        }
    }
}
