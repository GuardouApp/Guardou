using Guardou.Application.Files;
using Guardou.Domain.Files;
using Microsoft.EntityFrameworkCore;

namespace Guardou.Infrastructure.Persistence;

internal sealed class EfItemOpenStore(AppDbContext context) : IItemOpenStore
{
    private DbSet<ItemOpen> Opens => context.Set<ItemOpen>();

    public Task<ItemOpen?> FindAsync(Guid itemId, string userId, CancellationToken cancellationToken)
    {
        return Opens.SingleOrDefaultAsync(open => open.ItemId == itemId && open.UserId == userId, cancellationToken);
    }

    public async Task<IReadOnlyDictionary<Guid, DateTimeOffset>> OpenedAsync(
        string userId,
        IReadOnlyCollection<Guid> itemIds,
        CancellationToken cancellationToken)
    {
        return await Opens
            .Where(open => open.UserId == userId && itemIds.Contains(open.ItemId))
            .ToDictionaryAsync(open => open.ItemId, open => open.Opened, cancellationToken);
    }

    public void Add(ItemOpen open)
    {
        Opens.Add(open);
    }

    public Task SaveAsync(CancellationToken cancellationToken)
    {
        return context.SaveChangesAsync(cancellationToken);
    }
}
