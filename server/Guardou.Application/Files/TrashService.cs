using Guardou.Application.Shares;
using Guardou.Domain.Files;

namespace Guardou.Application.Files;

public sealed class TrashService
{
    private readonly IItemStore _store;
    private readonly OwnedItems _items;
    private readonly ItemPlacement _placement;
    private readonly ItemRemover _remover;
    private readonly ShareCleanup _shares;
    private readonly ItemEntryFactory _entries;
    private readonly TimeProvider _time;

    internal TrashService(
        IItemStore store,
        OwnedItems items,
        ItemPlacement placement,
        ItemRemover remover,
        ShareCleanup shares,
        ItemEntryFactory entries,
        TimeProvider time)
    {
        _store = store;
        _items = items;
        _placement = placement;
        _remover = remover;
        _shares = shares;
        _entries = entries;
        _time = time;
    }

    public async Task MoveToTrashAsync(string ownerId, IReadOnlyList<string> ids, CancellationToken cancellationToken)
    {
        DateTimeOffset now = _time.GetUtcNow();
        foreach (string id in ids)
        {
            ItemPath path = await _items.FindModifiableAsync(ownerId, id, cancellationToken);
            path.Item.Trash(now);
        }

        await _store.SaveAsync(cancellationToken);
        await _shares.RemoveInactiveAsync(ownerId, cancellationToken);
    }

    public async Task<IReadOnlyList<TrashEntry>> ListAsync(string ownerId, CancellationToken cancellationToken)
    {
        IReadOnlyList<Item> items = await _store.TrashedAsync(ownerId, cancellationToken);
        List<TrashEntry> entries = [];
        foreach (Item item in items.OrderByDescending(item => item.Trashed))
        {
            entries.Add(await ToEntryAsync(item, cancellationToken));
        }

        return entries;
    }

    public async Task<IReadOnlyList<ItemEntry>> RestoreAsync(
        string ownerId,
        IReadOnlyList<string> ids,
        CancellationToken cancellationToken)
    {
        List<ItemEntry> restored = [];
        foreach (string id in ids)
        {
            Item item = await _items.FindTrashedAsync(ownerId, id, cancellationToken);
            Item parent = await RestoreParentAsync(item, cancellationToken);
            string name = await _placement.ResolveNameAsync(parent, item.Name, ConflictPolicy.Rename, null, cancellationToken) ?? item.Name;
            item.Restore(parent, name);
            await _store.SaveAsync(cancellationToken);
            restored.Add(await _entries.CreateAsync(item, cancellationToken));
        }

        return restored;
    }

    public async Task DeleteAsync(string ownerId, IReadOnlyList<string> ids, CancellationToken cancellationToken)
    {
        List<Item> items = [];
        foreach (string id in ids)
        {
            items.Add(await _items.FindTrashedAsync(ownerId, id, cancellationToken));
        }

        await _remover.RemoveAsync(items, cancellationToken);
    }

    public async Task EmptyAsync(string ownerId, CancellationToken cancellationToken)
    {
        await _remover.RemoveAsync(await _store.TrashedAsync(ownerId, cancellationToken), cancellationToken);
    }

    public async Task PurgeExpiredAsync(CancellationToken cancellationToken)
    {
        DateTimeOffset cutoff = _time.GetUtcNow() - TrashPolicy.Retention;
        await _remover.RemoveAsync(await _store.TrashedBeforeAsync(cutoff, cancellationToken), cancellationToken);
    }

    private async Task<TrashEntry> ToEntryAsync(Item item, CancellationToken cancellationToken)
    {
        DateTimeOffset trashed = item.Trashed.GetValueOrDefault();
        return new TrashEntry(await _entries.CreateAsync(item, cancellationToken), trashed, TrashPolicy.ExpiresAt(trashed));
    }

    private async Task<Item> RestoreParentAsync(Item item, CancellationToken cancellationToken)
    {
        ItemPath? parent = item.ParentId is { } parentId
            ? await _items.TryFindActiveAsync(item.OwnerId, parentId, cancellationToken)
            : null;
        return parent is { Item.IsFolder: true } ? parent.Item : await _items.RootAsync(item.OwnerId, cancellationToken);
    }
}
