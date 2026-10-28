using System.Runtime.CompilerServices;
using Guardou.Application.Search;
using Guardou.Domain.Files;
using Microsoft.EntityFrameworkCore;

namespace Guardou.Infrastructure.Indexing;

internal sealed class EfItemIndex(IndexDbContext index) : IItemIndex
{
    private const string Wildcard = "%";

    public async Task<IReadOnlyDictionary<Guid, long>> FolderSizesAsync(
        IReadOnlyCollection<Guid> folderIds,
        CancellationToken cancellationToken)
    {
        return await index.Entries.AsNoTracking()
            .Where(entry => folderIds.Contains(entry.ItemId))
            .ToDictionaryAsync(entry => entry.ItemId, entry => entry.Size, cancellationToken);
    }

    public async IAsyncEnumerable<Guid> SearchAsync(
        SearchScope scope,
        SearchQuery query,
        int limit,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        List<string> paths = await index.Entries.AsNoTracking()
            .Where(entry => scope.Subtrees.Contains(entry.ItemId))
            .Select(entry => entry.Path)
            .ToListAsync(cancellationToken);
        if (paths.Count == 0)
        {
            yield break;
        }

        IQueryable<Guid> results = Filter(index.Entries.AsNoTracking().Where(IndexPath.WithinAny(paths)), scope, query)
            .Where(entry => entry.ItemId != scope.Excluded)
            .OrderByDescending(entry => entry.Modified)
            .ThenBy(entry => entry.Name)
            .Take(limit)
            .Select(entry => entry.ItemId);
        await foreach (Guid id in results.AsAsyncEnumerable().WithCancellation(cancellationToken))
        {
            yield return id;
        }
    }

    private IQueryable<IndexEntry> Filter(IQueryable<IndexEntry> entries, SearchScope scope, SearchQuery query)
    {
        foreach (string term in query.Terms)
        {
            entries = WithName(entries, term);
        }

        if (query.Types.Count > 0)
        {
            List<ItemType> types = query.Types.ToList();
            entries = entries.Where(entry => types.Contains(entry.Type));
        }

        if (query.ModifiedFrom is { } from)
        {
            entries = entries.Where(entry => entry.Modified >= from);
        }

        if (query.ModifiedBefore is { } before)
        {
            entries = entries.Where(entry => entry.Modified < before);
        }

        if (query.LargerThan is { } larger)
        {
            entries = entries.Where(entry => entry.Size > larger);
        }

        if (query.SmallerThan is { } smaller)
        {
            entries = entries.Where(entry => entry.Size < smaller);
        }

        if (query.Starred)
        {
            entries = entries.Where(entry => entry.Starred && entry.OwnerId == scope.ViewerId);
        }

        return entries;
    }

    private IQueryable<IndexEntry> WithName(IQueryable<IndexEntry> entries, string term)
    {
        string pattern = Wildcard + term.Replace(Wildcard, string.Empty, StringComparison.Ordinal) + Wildcard;
        IQueryable<long> matches = index.Names.Where(name => EF.Functions.Like(name.Name, pattern)).Select(name => name.RowId);
        return entries.Where(entry => matches.Contains(entry.Id));
    }
}
