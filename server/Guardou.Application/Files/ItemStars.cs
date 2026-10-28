using Guardou.Domain.Files;

namespace Guardou.Application.Files;

public sealed class ItemStars
{
    private readonly IItemStore _store;
    private readonly OwnedItems _items;
    private readonly ItemEntryFactory _entries;

    internal ItemStars(IItemStore store, OwnedItems items, ItemEntryFactory entries)
    {
        _store = store;
        _items = items;
        _entries = entries;
    }

    public Task StarAsync(string ownerId, IReadOnlyList<string> ids, CancellationToken cancellationToken)
    {
        return ChangeAsync(ownerId, ids, item => item.Star(), cancellationToken);
    }

    public Task UnstarAsync(string ownerId, IReadOnlyList<string> ids, CancellationToken cancellationToken)
    {
        return ChangeAsync(ownerId, ids, item => item.Unstar(), cancellationToken);
    }

    public async Task<IReadOnlyList<ItemEntry>> ListAsync(string ownerId, CancellationToken cancellationToken)
    {
        List<Item> active = [];
        foreach (Item item in await _store.StarredAsync(ownerId, cancellationToken))
        {
            if (await _items.TryFindActiveAsync(ownerId, item.Id, cancellationToken) is not null)
            {
                active.Add(item);
            }
        }

        return await _entries.CreateAsync(active, ownerId, cancellationToken);
    }

    private async Task ChangeAsync(string ownerId, IReadOnlyList<string> ids, Action<Item> change, CancellationToken cancellationToken)
    {
        foreach (string id in ids)
        {
            change((await _items.FindModifiableAsync(ownerId, id, cancellationToken)).Item);
        }

        await _store.SaveAsync(cancellationToken);
    }
}
