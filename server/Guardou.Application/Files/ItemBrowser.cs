using Guardou.Domain.Files;

namespace Guardou.Application.Files;

public sealed class ItemBrowser
{
    private readonly IItemStore _store;
    private readonly IBlobStore _blobs;
    private readonly OwnedItems _items;
    private readonly ItemEntryFactory _entries;

    internal ItemBrowser(IItemStore store, IBlobStore blobs, OwnedItems items, ItemEntryFactory entries)
    {
        _store = store;
        _blobs = blobs;
        _items = items;
        _entries = entries;
    }

    public async Task<ItemDetails> GetAsync(string userId, ItemDetailsRequest request, CancellationToken cancellationToken)
    {
        ItemAccess access = ItemAccess.Owned(await _items.FindActiveAsync(userId, request.Id, cancellationToken));
        return await DetailsAsync(access, request.Content, cancellationToken);
    }

    public async Task<IReadOnlyList<ItemEntry>> GetChildrenAsync(
        string userId,
        string id,
        ItemFilter? only,
        CancellationToken cancellationToken)
    {
        ItemAccess folder = ItemAccess.Owned(await _items.FindActiveFolderAsync(userId, id, cancellationToken));
        IReadOnlyList<Item> children = await _store.ChildrenAsync(folder.Item.Id, cancellationToken);
        return await _entries.CreateAsync(children.Where(child => Matches(child, only)), folder, cancellationToken);
    }

    internal async Task<ItemDetails> DetailsAsync(ItemAccess access, bool content, CancellationToken cancellationToken)
    {
        Item item = access.Item;
        ItemEntry entry = (await _entries.CreateAsync([item], access, cancellationToken)).Single();
        if (item.IsFolder)
        {
            IReadOnlyList<Item> children = await _store.ChildrenAsync(item.Id, cancellationToken);
            IReadOnlyList<ItemEntry> entries = await _entries.CreateAsync(children, access, cancellationToken);
            return new ItemDetails(entry, access.Breadcrumbs, entries, Content: null);
        }

        string? text = content ? await ReadTextAsync(item, cancellationToken) : null;
        return new ItemDetails(entry, access.Breadcrumbs, Items: null, text);
    }

    public async Task<StorageUsage> GetUsageAsync(string ownerId, CancellationToken cancellationToken)
    {
        return new StorageUsage(await _store.UsedBytesAsync(ownerId, cancellationToken), _blobs.TotalBytes());
    }

    private static bool Matches(Item item, ItemFilter? only)
    {
        return only switch
        {
            ItemFilter.Files => item.Kind == ItemKind.File,
            ItemFilter.Folders => item.Kind == ItemKind.Folder,
            _ => true,
        };
    }

    private async Task<string?> ReadTextAsync(Item item, CancellationToken cancellationToken)
    {
        if (item.Size > TextContentReader.SizeLimit || item.StorageKey is not { } key)
        {
            return null;
        }

        await using Stream stream = _blobs.OpenRead(key);
        return await TextContentReader.ReadAsync(stream, cancellationToken);
    }
}
