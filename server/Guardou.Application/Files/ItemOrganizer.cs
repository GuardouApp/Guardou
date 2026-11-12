using Guardou.Application.Persistence;
using Guardou.Application.Quotas;
using Guardou.Domain.Files;

namespace Guardou.Application.Files;

public sealed class ItemOrganizer
{
    private readonly IItemStore _store;
    private readonly OwnedItems _items;
    private readonly ItemPlacement _placement;
    private readonly ItemTree _tree;
    private readonly ItemEntryFactory _entries;
    private readonly CopyTasks _copies;
    private readonly StorageReservations _reservations;
    private readonly IWriteTransactions _transactions;
    private readonly TimeProvider _time;

    internal ItemOrganizer(
        IItemStore store,
        OwnedItems items,
        ItemPlacement placement,
        ItemTree tree,
        ItemEntryFactory entries,
        CopyTasks copies,
        StorageReservations reservations,
        IWriteTransactions transactions,
        TimeProvider time)
    {
        _store = store;
        _items = items;
        _placement = placement;
        _tree = tree;
        _entries = entries;
        _copies = copies;
        _reservations = reservations;
        _transactions = transactions;
        _time = time;
    }

    public async Task<ItemEntry> CreateFolderAsync(string ownerId, string parentId, string name, CancellationToken cancellationToken)
    {
        ItemPath parent = await _items.FindActiveFolderAsync(ownerId, parentId, cancellationToken);
        string validName = ItemPlacement.ValidName(name);
        await _placement.ResolveNameAsync(parent.Item, validName, ConflictPolicy.Fail, null, cancellationToken);
        Item folder = Item.CreateFolder(parent.Item, validName, ownerId, _time.GetUtcNow());
        _store.Add(folder);
        await _store.SaveAsync(cancellationToken);
        return await _entries.CreateAsync(folder, ownerId, cancellationToken);
    }

    public async Task<ItemEntry> RenameAsync(string ownerId, string id, string name, CancellationToken cancellationToken)
    {
        ItemPath path = await _items.FindModifiableAsync(ownerId, id, cancellationToken);
        Item item = path.Item;
        Item parent = path.Ancestors[^1];
        string validName = ItemPlacement.ValidName(name);
        if (item.Name != validName)
        {
            await _placement.ResolveNameAsync(parent, validName, ConflictPolicy.Fail, path, cancellationToken);
            item.Rename(validName, _time.GetUtcNow());
            await _store.SaveAsync(cancellationToken);
        }

        return await _entries.CreateAsync(item, ownerId, cancellationToken);
    }

    public async Task<ItemEntry?> MoveAsync(
        string ownerId,
        string id,
        string destinationId,
        ConflictPolicy conflict,
        CancellationToken cancellationToken)
    {
        Item? item = await _transactions.RunAsync(
            () => PlaceMovedAsync(ownerId, id, destinationId, conflict, cancellationToken),
            cancellationToken);
        return item is null ? null : await _entries.CreateAsync(item, ownerId, cancellationToken);
    }

    private async Task<Item?> PlaceMovedAsync(
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
            return item;
        }

        string? name = await _placement.ResolveNameAsync(destination.Item, item.Name, conflict, source, cancellationToken);
        if (name is null)
        {
            return null;
        }

        item.MoveTo(destination.Item, name);
        await _store.SaveAsync(cancellationToken);
        return item;
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

        long size = await ActiveSizeAsync(source.Item, cancellationToken);
        IReadOnlyList<Item>? copies = await _reservations.ReserveAsync(
            ownerId,
            size,
            () => PlaceCopyAsync(source, destination.Item, conflict, ownerId, cancellationToken),
            cancellationToken);
        if (copies is null)
        {
            return null;
        }

        await _copies.EnqueueAsync(copies, cancellationToken);
        return await _entries.CreateAsync(copies[0], ownerId, cancellationToken);
    }

    private async Task<long> ActiveSizeAsync(Item item, CancellationToken cancellationToken)
    {
        IReadOnlyList<TreeNode> nodes = await _tree.ActiveSubtreeAsync(item, item.Name, cancellationToken);
        return nodes.Sum(node => node.Item.Size);
    }

    private async Task<IReadOnlyList<Item>?> PlaceCopyAsync(
        ItemPath source,
        Item destination,
        ConflictPolicy conflict,
        string createdBy,
        CancellationToken cancellationToken)
    {
        string? name = await _placement.ResolveNameAsync(destination, source.Item.Name, conflict, source, cancellationToken);
        return name is null ? null : await CopyTreeAsync(source.Item, destination, name, createdBy, cancellationToken);
    }

    private async Task<IReadOnlyList<Item>> CopyTreeAsync(
        Item source,
        Item destination,
        string name,
        string createdBy,
        CancellationToken cancellationToken)
    {
        CopyContext context = new(createdBy, _time.GetUtcNow());
        List<Item> copies = [];
        Dictionary<Guid, Item> copiesBySource = [];
        foreach (Item item in await _store.ActiveSubtreeAsync(source.Id, cancellationToken))
        {
            Item copy = item.Id == source.Id || item.ParentId is not { } parentId
                ? CopyItem(item, destination, name, context)
                : CopyItem(item, copiesBySource[parentId], item.Name, context);
            copiesBySource[item.Id] = copy;
            copies.Add(copy);
        }

        await _store.SaveAsync(cancellationToken);
        return copies;
    }

    private Item CopyItem(Item source, Item parent, string name, CopyContext context)
    {
        Item copy = source.StorageKey is { } storageKey
            ? Item.CreateCopy(parent, NewFile(source, name), source.CopySourceKey ?? storageKey, context.CreatedBy, context.Now)
            : Item.CreateFolder(parent, name, context.CreatedBy, context.Now);
        _store.Add(copy);
        return copy;
    }

    private static StoredFile NewFile(Item source, string name)
    {
        Guid id = Guid.CreateVersion7();
        return new StoredFile(id, name, source.Size, StorageKey.For(source.OwnerId, id, name, source.Modified), source.Modified);
    }

    private sealed record CopyContext(string CreatedBy, DateTimeOffset Now);
}
