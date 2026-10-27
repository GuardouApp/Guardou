using Guardou.Domain.Files;

namespace Guardou.Application.Files;

public sealed class RecentItems
{
    internal const int Limit = 20;

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
        List<ItemEntry> entries = [];
        int skip = 0;
        IReadOnlyList<Item> page;
        do
        {
            page = await _store.RecentFilesAsync(userId, skip, Limit, cancellationToken);
            skip += page.Count;
            foreach (Item item in page)
            {
                if (entries.Count < Limit && await _items.TryFindActiveAsync(userId, item.Id, cancellationToken) is { } access)
                {
                    entries.Add((await _entries.CreateAsync([item], access, cancellationToken)).Single());
                }
            }
        }
        while (page.Count == Limit && entries.Count < Limit);

        return entries;
    }
}
