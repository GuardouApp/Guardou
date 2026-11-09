using Guardou.Application.Quotas;
using Guardou.Domain.Files;
using Guardou.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Guardou.Infrastructure.Persistence;

internal sealed class EfStorageQuotaStore(AppDbContext context) : IStorageQuotaStore
{
    public async Task<StorageQuota> FindAsync(string ownerId, CancellationToken cancellationToken)
    {
        StorageQuota quota = await context.Users
            .Where(user => user.Id == ownerId)
            .Select(user => new StorageQuota(user.QuotaBytes, user.UsedBytes, 0))
            .SingleAsync(cancellationToken);
        long reserved = await context.Set<Upload>().Where(upload => upload.OwnerId == ownerId).SumAsync(upload => upload.Size, cancellationToken);
        return quota with { ReservedBytes = reserved };
    }
}
