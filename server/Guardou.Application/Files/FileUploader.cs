using Guardou.Application.Previews;
using Guardou.Domain.Files;
using Guardou.Domain.Tasks;

namespace Guardou.Application.Files;

public sealed class FileUploader
{
    private static readonly char[] PathSeparators = ['/', '\\'];

    private readonly IItemStore _store;
    private readonly IBlobStore _blobs;
    private readonly OwnedItems _items;
    private readonly ItemPlacement _placement;
    private readonly ItemEntryFactory _entries;
    private readonly PreviewTasks _previews;
    private readonly TimeProvider _time;

    internal FileUploader(
        IItemStore store,
        IBlobStore blobs,
        OwnedItems items,
        ItemPlacement placement,
        ItemEntryFactory entries,
        PreviewTasks previews,
        TimeProvider time)
    {
        _store = store;
        _blobs = blobs;
        _items = items;
        _placement = placement;
        _entries = entries;
        _previews = previews;
        _time = time;
    }

    public async Task<ItemEntry?> UploadAsync(string ownerId, UploadRequest request, CancellationToken cancellationToken)
    {
        string name = ItemPlacement.ValidName(request.FileName);
        ItemPath target = await _items.FindActiveFolderAsync(ownerId, request.FolderId, cancellationToken);
        Item folder = await EnsureFoldersAsync(target.Item, FolderNames(request.RelativePath), ownerId, cancellationToken);
        string? placedName = await _placement.ResolveNameAsync(folder, name, request.Conflict, null, cancellationToken);
        if (placedName is null)
        {
            return null;
        }

        DateTimeOffset now = _time.GetUtcNow();
        StoredFile file = await StoreAsync(folder.OwnerId, placedName, request.Modified ?? now, request.Content, cancellationToken);
        Item item = Item.CreateFile(folder, file, ownerId, now);
        _store.Add(item);
        try
        {
            await _store.SaveAsync(cancellationToken);
        }
        catch
        {
            _blobs.Delete(file.StorageKey);
            throw;
        }

        await _previews.EnqueueAsync([item], TaskPriority.Normal, cancellationToken);
        return await _entries.CreateAsync(item, cancellationToken);
    }

    private static List<string> FolderNames(string? relativePath)
    {
        string[] segments = (relativePath ?? string.Empty).Split(PathSeparators, StringSplitOptions.RemoveEmptyEntries);
        return segments.SkipLast(1).Select(ItemPlacement.ValidName).ToList();
    }

    private async Task<Item> EnsureFoldersAsync(
        Item folder,
        IReadOnlyList<string> names,
        string createdBy,
        CancellationToken cancellationToken)
    {
        foreach (string name in names)
        {
            Item? existing = await _store.FindChildAsync(folder.Id, name, cancellationToken);
            if (existing is { IsFolder: false })
            {
                throw new ItemConflictException(name);
            }

            if (existing is null)
            {
                existing = Item.CreateFolder(folder, name, createdBy, _time.GetUtcNow());
                _store.Add(existing);
                await _store.SaveAsync(cancellationToken);
            }

            folder = existing;
        }

        return folder;
    }

    private async Task<StoredFile> StoreAsync(
        string ownerId,
        string name,
        DateTimeOffset modified,
        Stream content,
        CancellationToken cancellationToken)
    {
        Guid id = Guid.CreateVersion7();
        string key = StorageKey.For(ownerId, id, name, modified);
        long size = await _blobs.WriteAsync(key, content, modified, cancellationToken);
        return new StoredFile(id, name, size, key, modified);
    }
}
