using Guardou.Application.Files;
using Guardou.Application.Tasks;
using Guardou.Domain.Previews;
using Guardou.Domain.Tasks;

namespace Guardou.Application.Previews;

internal sealed class PreviewTaskHandler(IItemStore items, IPreviewRenderer renderer, IPreviewCache cache) : ITaskHandler
{
    public TaskKind Kind => TaskKind.Preview;

    public async Task ExecuteAsync(QueuedTask task, CancellationToken cancellationToken)
    {
        if (await items.FindAnyAsync(task.ItemId, cancellationToken) is not { StorageKey: { } key } item
            || cache.Contains(item.OwnerId, item.Id))
        {
            return;
        }

        IReadOnlyDictionary<PreviewSize, byte[]> previews = await renderer.RenderAsync(key, item.Name, cancellationToken);
        foreach ((PreviewSize size, byte[] preview) in previews)
        {
            await cache.WriteAsync(item.OwnerId, item.Id, size, preview, cancellationToken);
        }
    }
}
