using Guardou.Application.Files;
using Guardou.Application.Tasks;
using Guardou.Domain.Files;
using Guardou.Domain.Previews;
using Guardou.Domain.Tasks;

namespace Guardou.Application.Previews;

public sealed class ItemPreviews
{
    private readonly OwnedItems _items;
    private readonly ITaskStore _tasks;
    private readonly IPreviewRenderer _renderer;
    private readonly IPreviewCache _cache;
    private readonly PreviewTasks _previewTasks;

    internal ItemPreviews(OwnedItems items, ITaskStore tasks, IPreviewRenderer renderer, IPreviewCache cache, PreviewTasks previewTasks)
    {
        _items = items;
        _tasks = tasks;
        _renderer = renderer;
        _cache = cache;
        _previewTasks = previewTasks;
    }

    public async Task<CachedPreview?> OpenCurrentAsync(string ownerId, string id, PreviewSize size, CancellationToken cancellationToken)
    {
        Item item = await FindPreviewableAsync(ownerId, id, cancellationToken);
        if (item.PreviewId is { } previewId && _cache.TryOpen(item.OwnerId, previewId, size) is { } content)
        {
            return new CachedPreview(ItemId.Format(previewId), content);
        }

        if (await _tasks.FindStateAsync(TaskKind.Preview, item.Id, cancellationToken) == TaskState.Failed)
        {
            throw new ItemNotFoundException($"The preview of the item '{id}' failed.");
        }

        await _previewTasks.EnqueueAsync([item], TaskPriority.High, cancellationToken);
        return null;
    }

    public async Task<CachedPreview> OpenAsync(
        string ownerId,
        string id,
        string previewId,
        PreviewSize size,
        CancellationToken cancellationToken)
    {
        Item item = await FindPreviewableAsync(ownerId, id, cancellationToken);
        return ItemId.TryParse(previewId, out Guid requested)
            && item.PreviewId == requested
            && _cache.TryOpen(item.OwnerId, requested, size) is { } content
                ? new CachedPreview(previewId, content)
                : throw new ItemNotFoundException($"The preview '{previewId}' of the item '{id}' does not exist.");
    }

    private async Task<Item> FindPreviewableAsync(string ownerId, string id, CancellationToken cancellationToken)
    {
        Item item = (await _items.FindActiveAsync(ownerId, id, cancellationToken)).Item;
        return item.IsFolder || !_renderer.Supports(item.Name)
            ? throw new ItemNotFoundException($"The item '{id}' has no preview.")
            : item;
    }
}
