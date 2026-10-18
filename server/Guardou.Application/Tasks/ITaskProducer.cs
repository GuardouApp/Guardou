using Guardou.Domain.Tasks;

namespace Guardou.Application.Tasks;

public interface ITaskProducer
{
    TaskKind Kind { get; }

    Task EnqueueAllAsync(CancellationToken cancellationToken);
}
