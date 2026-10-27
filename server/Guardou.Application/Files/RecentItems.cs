using Guardou.Domain.Files;

namespace Guardou.Application.Files;

public sealed class RecentItems
{
    internal const int FolderLimit = 12;
    internal const int FileLimit = 20;

    private readonly IItemStore _store;
    private readonly AccessibleItems _items;
    private readonly ItemEntryFactory _entries;

    internal RecentItems(IItemStore store, AccessibleItems items, ItemEntryFactory entries)
    {
        _store = store;
        _items = items;
        _entries = entries;
    }

    public async Task<IReadOnlyList<ItemEntry>> ListAsync(string userId, CancellationToken cancellationToken)
    {
        IReadOnlyList<ItemEntry> folders = await ListAsync(userId, ItemKind.Folder, FolderLimit, cancellationToken);
        IReadOnlyList<ItemEntry> files = await ListAsync(userId, ItemKind.File, FileLimit, cancellationToken);
        return [.. folders, .. files];
    }

    private async Task<IReadOnlyList<ItemEntry>> ListAsync(
        string userId,
        ItemKind kind,
        int limit,
        CancellationToken cancellationToken)
    {
        List<ItemEntry> entries = [];
        int skip = 0;
        IReadOnlyList<Item> page;
        do
        {
            page = await _store.RecentAsync(userId, kind, skip, limit, cancellationToken);
            skip += page.Count;
            foreach (Item item in page)
            {
                if (entries.Count < limit && await _items.TryFindActiveAsync(userId, item.Id, cancellationToken) is { } access)
                {
                    entries.Add((await _entries.CreateAsync([item], access, cancellationToken)).Single());
                }
            }
        }
        while (page.Count == limit && entries.Count < limit);

        return entries;
    }
}
