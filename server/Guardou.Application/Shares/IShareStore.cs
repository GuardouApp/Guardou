using Guardou.Domain.Shares;

namespace Guardou.Application.Shares;

public interface IShareStore
{
    Task<Share?> FindByItemAsync(Guid itemId, CancellationToken cancellationToken);

    Task<Share?> FindByTokenAsync(string token, CancellationToken cancellationToken);

    Task<IReadOnlyList<Share>> OwnedAsync(string ownerId, CancellationToken cancellationToken);

    void Add(Share share);

    void Remove(Share share);

    Task SaveAsync(CancellationToken cancellationToken);
}
