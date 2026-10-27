using Guardou.Domain.Files;

namespace Guardou.Application.Files;

public interface IItemOpenStore
{
    Task<ItemOpen?> FindAsync(Guid itemId, string userId, CancellationToken cancellationToken);

    Task<IReadOnlyDictionary<Guid, DateTimeOffset>> OpenedAsync(
        string userId,
        IReadOnlyCollection<Guid> itemIds,
        CancellationToken cancellationToken);

    void Add(ItemOpen open);

    Task SaveAsync(CancellationToken cancellationToken);
}
