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

    public async Task<ItemDetails> GetAsync(string ownerId, ItemDetailsRequest request, CancellationToken cancellationToken)
    {
        ItemPath path = await _items.FindActiveAsync(ownerId, request.Id, cancellationToken);
        Item item = path.Item;
        IReadOnlyList<Breadcrumb> ancestors = path.Ancestors.Select(ItemEntryFactory.Breadcrumb).ToList();
        if (item.IsFolder)
        {
            IReadOnlyList<Item> children = await _store.ChildrenAsync(item.Id, cancellationToken);
            return new ItemDetails(
                await _entries.CreateAsync(item, cancellationToken),
                ancestors,
                await _entries.CreateAsync(children, cancellationToken),
                Content: null);
        }

        string? content = request.Content ? await ReadTextAsync(item, cancellationToken) : null;
        return new ItemDetails(await _entries.CreateAsync(item, cancellationToken), ancestors, Items: null, content);
    }

    public async Task<IReadOnlyList<ItemEntry>> GetChildrenAsync(
        string ownerId,
        string id,
        ItemFilter? only,
        CancellationToken cancellationToken)
    {
        ItemPath folder = await _items.FindActiveFolderAsync(ownerId, id, cancellationToken);
        IReadOnlyList<Item> children = await _store.ChildrenAsync(folder.Item.Id, cancellationToken);
        return await _entries.CreateAsync(children.Where(child => Matches(child, only)), cancellationToken);
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
