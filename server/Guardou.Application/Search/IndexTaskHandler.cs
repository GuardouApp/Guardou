using Guardou.Application.Tasks;
using Guardou.Domain.Tasks;

namespace Guardou.Application.Search;

internal sealed class IndexTaskHandler(IItemIndexRebuilder index) : ITaskHandler
{
    public TaskKind Kind => TaskKind.SearchIndex;

    public Task ExecuteAsync(QueuedTask task, CancellationToken cancellationToken)
    {
        return index.RebuildAsync(task.ItemId, cancellationToken);
    }
}
