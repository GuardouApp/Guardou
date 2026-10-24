using Guardou.Domain.Shares;

namespace Guardou.Application.Shares;

public interface IDirectShareStore
{
    Task<DirectShare?> FindAsync(Guid itemId, string userId, CancellationToken cancellationToken);

    Task<IReadOnlyList<DirectShare>> OfItemAsync(Guid itemId, CancellationToken cancellationToken);

    Task<IReadOnlyList<DirectShare>> OfUserAsync(string userId, CancellationToken cancellationToken);

    Task<IReadOnlyList<DirectShare>> OwnedAsync(string ownerId, CancellationToken cancellationToken);

    void Add(DirectShare share);

    void Remove(DirectShare share);

    Task SaveAsync(CancellationToken cancellationToken);
}
