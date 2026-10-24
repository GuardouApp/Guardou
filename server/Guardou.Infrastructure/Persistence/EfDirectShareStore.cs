using Guardou.Application.Shares;
using Guardou.Domain.Files;
using Guardou.Domain.Shares;
using Microsoft.EntityFrameworkCore;

namespace Guardou.Infrastructure.Persistence;

internal sealed class EfDirectShareStore(AppDbContext context) : IDirectShareStore
{
    private DbSet<DirectShare> Shares => context.Set<DirectShare>();

    public Task<DirectShare?> FindAsync(Guid itemId, string userId, CancellationToken cancellationToken)
    {
        return Shares.SingleOrDefaultAsync(share => share.ItemId == itemId && share.UserId == userId, cancellationToken);
    }

    public async Task<IReadOnlyList<DirectShare>> OfItemAsync(Guid itemId, CancellationToken cancellationToken)
    {
        return await Shares.Where(share => share.ItemId == itemId).ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<DirectShare>> OfUserAsync(string userId, CancellationToken cancellationToken)
    {
        return await Shares.Where(share => share.UserId == userId).ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<DirectShare>> OwnedAsync(string ownerId, CancellationToken cancellationToken)
    {
        return await Shares
            .Where(share => context.Set<Item>().Any(item => item.Id == share.ItemId && item.OwnerId == ownerId))
            .ToListAsync(cancellationToken);
    }

    public void Add(DirectShare share)
    {
        Shares.Add(share);
    }

    public void Remove(DirectShare share)
    {
        Shares.Remove(share);
    }

    public Task SaveAsync(CancellationToken cancellationToken)
    {
        return context.SaveChangesAsync(cancellationToken);
    }
}
