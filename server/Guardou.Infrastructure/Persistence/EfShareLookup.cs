using Guardou.Application.Shares;
using Guardou.Domain.Shares;
using Microsoft.EntityFrameworkCore;

namespace Guardou.Infrastructure.Persistence;

internal sealed class EfShareLookup(AppDbContext context) : IShareLookup
{
    public async Task<IReadOnlySet<Guid>> SharedAsync(IReadOnlyCollection<Guid> itemIds, CancellationToken cancellationToken)
    {
        List<Guid> shared = await context.Set<Share>()
            .Where(share => itemIds.Contains(share.ItemId))
            .Select(share => share.ItemId)
            .Union(context.Set<DirectShare>()
                .Where(share => itemIds.Contains(share.ItemId))
                .Select(share => share.ItemId))
            .ToListAsync(cancellationToken);
        return shared.ToHashSet();
    }
}
