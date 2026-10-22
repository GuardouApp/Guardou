using Guardou.Application.Shares;
using Guardou.Domain.Files;
using Guardou.Domain.Shares;
using Microsoft.EntityFrameworkCore;

namespace Guardou.Infrastructure.Persistence;

internal sealed class EfShareStore(AppDbContext context) : IShareStore
{
    private DbSet<Share> Shares => context.Set<Share>();

    public Task<Share?> FindByItemAsync(Guid itemId, CancellationToken cancellationToken)
    {
        return Shares.SingleOrDefaultAsync(share => share.ItemId == itemId, cancellationToken);
    }

    public Task<Share?> FindByTokenAsync(string token, CancellationToken cancellationToken)
    {
        return Shares.SingleOrDefaultAsync(share => share.Token == token, cancellationToken);
    }

    public async Task<IReadOnlyList<Share>> OwnedAsync(string ownerId, CancellationToken cancellationToken)
    {
        return await Shares
            .Where(share => context.Set<Item>().Any(item => item.Id == share.ItemId && item.OwnerId == ownerId))
            .ToListAsync(cancellationToken);
    }

    public void Add(Share share)
    {
        Shares.Add(share);
    }

    public void Remove(Share share)
    {
        Shares.Remove(share);
    }

    public Task SaveAsync(CancellationToken cancellationToken)
    {
        return context.SaveChangesAsync(cancellationToken);
    }
}
