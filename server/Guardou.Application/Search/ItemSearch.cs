using System.Runtime.CompilerServices;
using Guardou.Application.Files;
using Guardou.Application.Shares;

namespace Guardou.Application.Search;

public sealed class ItemSearch
{
    public const int DefaultLimit = 50;
    public const int MaximumLimit = 200;

    private readonly IItemIndex _index;
    private readonly OwnedItems _owned;
    private readonly AccessibleItems _items;
    private readonly IDirectShareStore _shares;
    private readonly ItemEntryFactory _entries;
    private readonly SearchQueryParser _parser;

    internal ItemSearch(
        IItemIndex index,
        OwnedItems owned,
        AccessibleItems items,
        IDirectShareStore shares,
        ItemEntryFactory entries,
        TimeProvider time)
    {
        _index = index;
        _owned = owned;
        _items = items;
        _shares = shares;
        _entries = entries;
        _parser = new SearchQueryParser(time);
    }

    public async Task<IAsyncEnumerable<ItemEntry>> SearchAsync(
        string userId,
        string? text,
        string? folderId,
        int limit,
        CancellationToken cancellationToken)
    {
        SearchQuery query = _parser.Parse(text);
        SearchScope scope = await ScopeAsync(userId, folderId, cancellationToken);
        return query.IsEmpty ? AsyncEnumerable.Empty<ItemEntry>() : ResultsAsync(userId, scope, query, limit, cancellationToken);
    }

    private async IAsyncEnumerable<ItemEntry> ResultsAsync(
        string userId,
        SearchScope scope,
        SearchQuery query,
        int limit,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await foreach (Guid id in _index.SearchAsync(scope, query, limit, cancellationToken))
        {
            if (await _items.TryFindActiveAsync(userId, id, cancellationToken) is { } access)
            {
                yield return (await _entries.CreateAsync([access.Item], access, cancellationToken)).Single();
            }
        }
    }

    private async Task<SearchScope> ScopeAsync(string userId, string? folderId, CancellationToken cancellationToken)
    {
        if (folderId is not null)
        {
            ItemAccess folder = await _items.FindActiveFolderAsync(userId, folderId, cancellationToken);
            return new SearchScope([folder.Item.Id], folder.Item.Id);
        }

        Guid rootId = (await _owned.RootAsync(userId, cancellationToken)).Id;
        IEnumerable<Guid> shared = (await _shares.OfUserAsync(userId, cancellationToken)).Select(share => share.ItemId);
        return new SearchScope([rootId, .. shared], rootId);
    }
}
