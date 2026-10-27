namespace Guardou.Application.Search;

public interface IItemIndexRebuilder
{
    Task RebuildAsync(Guid rootId, CancellationToken cancellationToken);
}
