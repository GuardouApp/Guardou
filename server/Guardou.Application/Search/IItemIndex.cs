namespace Guardou.Application.Search;

public interface IItemIndex
{
    Task<IReadOnlyDictionary<Guid, long>> FolderSizesAsync(IReadOnlyCollection<Guid> folderIds, CancellationToken cancellationToken);

    IAsyncEnumerable<Guid> SearchAsync(SearchScope scope, SearchQuery query, int limit, CancellationToken cancellationToken);
}
