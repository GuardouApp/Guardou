using Guardou.Application.Previews;
using Guardou.Domain.Files;
using Guardou.Domain.Tasks;

namespace Guardou.Application.Files;

public sealed class ItemOrganizer
{
    private readonly IItemStore _store;
    private readonly IBlobStore _blobs;
    private readonly OwnedItems _items;
    private readonly ItemPlacement _placement;
    private readonly ItemTree _tree;
    private readonly ItemEntryFactory _entries;
    private readonly PreviewTasks _previews;
    private readonly TimeProvider _time;

    internal ItemOrganizer(
        IItemStore store,
        IBlobStore blobs,
        OwnedItems items,
        ItemPlacement placement,
        ItemTree tree,
        ItemEntryFactory entries,
        PreviewTasks previews,
        TimeProvider time)
    {
        _store = store;
        _blobs = blobs;
        _items = items;
        _placement = placement;
        _tree = tree;
        _entries = entries;
        _previews = previews;
        _time = time;
    }

    public async Task<ItemEntry> CreateFolderAsync(string ownerId, string parentId, string name, CancellationToken cancellationToken)
    {
        ItemPath parent = await _items.FindActiveFolderAsync(ownerId, parentId, cancellationToken);
        await _placement.ResolveNameAsync(parent.Item, ItemPlacement.ValidName(name), ConflictPolicy.Fail, null, cancellationToken);
        Item folder = Item.CreateFolder(parent.Item, name, _time.GetUtcNow());
        _store.Add(folder);
        await _store.SaveAsync(cancellationToken);
        return _entries.Create(folder);
    }

    public async Task<ItemEntry> RenameAsync(string ownerId, string id, string name, CancellationToken cancellationToken)
    {
        ItemPath path = await _items.FindModifiableAsync(ownerId, id, cancellationToken);
        Item item = path.Item;
        Item parent = path.Ancestors[^1];
        if (item.Name != ItemPlacement.ValidName(name))
        {
            await _placement.ResolveNameAsync(parent, name, ConflictPolicy.Fail, path, cancellationToken);
            item.Rename(name, _time.GetUtcNow());
            await _store.SaveAsync(cancellationToken);
        }

        return _entries.Create(item);
    }

    public async Task<ItemEntry?> MoveAsync(
        string ownerId,
        string id,
        string destinationId,
        ConflictPolicy conflict,
        CancellationToken cancellationToken)
    {
        ItemPath source = await _items.FindModifiableAsync(ownerId, id, cancellationToken);
        ItemPath destination = await _items.FindActiveFolderAsync(ownerId, destinationId, cancellationToken);
        Item item = source.Item;
        if (destination.Contains(item.Id))
        {
            throw new InvalidItemRequestException("A folder cannot move into itself.");
        }

        if (item.ParentId == destination.Item.Id)
        {
            return _entries.Create(item);
        }

        string? name = await _placement.ResolveNameAsync(destination.Item, item.Name, conflict, source, cancellationToken);
        if (name is null)
        {
            return null;
        }

        item.MoveTo(destination.Item, name);
        await _store.SaveAsync(cancellationToken);
        return _entries.Create(item);
    }

    public async Task<ItemEntry?> CopyAsync(
        string ownerId,
        string id,
        string destinationId,
        ConflictPolicy conflict,
        CancellationToken cancellationToken)
    {
        ItemPath source = await _items.FindModifiableAsync(ownerId, id, cancellationToken);
        ItemPath destination = await _items.FindActiveFolderAsync(ownerId, destinationId, cancellationToken);
        if (destination.Contains(source.Item.Id))
        {
            throw new InvalidItemRequestException("A folder cannot copy into itself.");
        }

        string? name = await _placement.ResolveNameAsync(destination.Item, source.Item.Name, conflict, source, cancellationToken);
        if (name is null)
        {
            return null;
        }

        Item copy = await CopyTreeAsync(source.Item, destination.Item, name, cancellationToken);
        return _entries.Create(copy);
    }

    private async Task<Item> CopyTreeAsync(Item source, Item destination, string name, CancellationToken cancellationToken)
    {
        DateTimeOffset now = _time.GetUtcNow();
        List<string> written = [];
        List<Item> copies = [];
        try
        {
            Item copy = await CopyItemAsync(source, destination, name, now, written, cancellationToken);
            copies.Add(copy);
            Queue<(Item Source, Item Copy)> folders = new([(source, copy)]);
            while (folders.TryDequeue(out (Item Source, Item Copy) folder))
            {
                foreach (Item child in await _store.ChildrenAsync(folder.Source.Id, cancellationToken))
                {
                    Item childCopy = await CopyItemAsync(child, folder.Copy, child.Name, now, written, cancellationToken);
                    copies.Add(childCopy);
                    if (child.IsFolder)
                    {
                        folders.Enqueue((child, childCopy));
                    }
                }
            }

            await _store.SaveAsync(cancellationToken);
        }
        catch
        {
            written.ForEach(_blobs.Delete);
            throw;
        }

        await _previews.EnqueueAsync(copies, TaskPriority.Normal, cancellationToken);
        return copies[0];
    }

    private async Task<Item> CopyItemAsync(
        Item source,
        Item parent,
        string name,
        DateTimeOffset now,
        List<string> written,
        CancellationToken cancellationToken)
    {
        Item copy = source.StorageKey is { } sourceKey
            ? Item.CreateFile(parent, await CopyBlobAsync(source, sourceKey, name, written, cancellationToken), now)
            : Item.CreateFolder(parent, name, now);
        _store.Add(copy);
        return copy;
    }

    private async Task<StoredFile> CopyBlobAsync(
        Item source,
        string sourceKey,
        string name,
        List<string> written,
        CancellationToken cancellationToken)
    {
        Guid id = Guid.CreateVersion7();
        string key = StorageKey.For(source.OwnerId, id, name, source.Modified);
        await _blobs.CopyAsync(sourceKey, key, cancellationToken);
        written.Add(key);
        return new StoredFile(id, name, source.Size, key, source.Modified);
    }
}
