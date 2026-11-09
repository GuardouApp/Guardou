using Guardou.Domain.Files;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Guardou.Infrastructure.Persistence;

internal sealed class StorageUsageCounter(AppDbContext context)
{
    public IReadOnlyDictionary<string, long> PendingChanges()
    {
        return context.ChangeTracker.Entries<Item>()
            .Select(entry => (entry.Entity.OwnerId, Bytes: BytesOf(entry)))
            .Where(change => change.Bytes != 0)
            .GroupBy(change => change.OwnerId)
            .ToDictionary(owner => owner.Key, owner => owner.Sum(change => change.Bytes));
    }

    public async Task ApplyAsync(IReadOnlyDictionary<string, long> changes, CancellationToken cancellationToken)
    {
        foreach ((string ownerId, long bytes) in changes)
        {
            await context.Users
                .Where(user => user.Id == ownerId)
                .ExecuteUpdateAsync(setters => setters.SetProperty(user => user.UsedBytes, user => user.UsedBytes + bytes), cancellationToken);
        }
    }

    private static long BytesOf(EntityEntry<Item> entry)
    {
        return entry.State switch
        {
            EntityState.Added => entry.Entity.Size,
            EntityState.Deleted => -entry.Entity.Size,
            _ => 0,
        };
    }
}
