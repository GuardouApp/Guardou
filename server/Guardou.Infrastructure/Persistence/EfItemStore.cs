using Guardou.Application.Files;
using Guardou.Domain.Files;
using Microsoft.EntityFrameworkCore;

namespace Guardou.Infrastructure.Persistence;

internal sealed class EfItemStore(AppDbContext context) : IItemStore
{
    private DbSet<Item> Items => context.Set<Item>();

    public Task<Item?> FindAsync(string ownerId, Guid id, CancellationToken cancellationToken)
    {
        return Items.SingleOrDefaultAsync(item => item.Id == id && item.OwnerId == ownerId, cancellationToken);
    }

    public Task<Item?> FindAnyAsync(Guid id, CancellationToken cancellationToken)
    {
        return Items.SingleOrDefaultAsync(item => item.Id == id, cancellationToken);
    }

    public Task<Item?> FindRootAsync(string ownerId, CancellationToken cancellationToken)
    {
        return Items.SingleOrDefaultAsync(item => item.OwnerId == ownerId && item.ParentId == null, cancellationToken);
    }

    public Task<Item?> FindChildAsync(Guid parentId, string name, CancellationToken cancellationToken)
    {
        return Items.SingleOrDefaultAsync(
            item => item.ParentId == parentId && item.Name == name && item.Trashed == null,
            cancellationToken);
    }

    public async Task<IReadOnlyList<Item>> ChildrenAsync(Guid parentId, CancellationToken cancellationToken)
    {
        return await Items.Where(item => item.ParentId == parentId && item.Trashed == null).ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Item>> ChildrenWithTrashAsync(Guid parentId, CancellationToken cancellationToken)
    {
        return await Items.Where(item => item.ParentId == parentId).ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Item>> TrashedAsync(string ownerId, CancellationToken cancellationToken)
    {
        return await Items.Where(item => item.OwnerId == ownerId && item.Trashed != null).ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Item>> TrashedBeforeAsync(DateTimeOffset cutoff, CancellationToken cancellationToken)
    {
        return await Items.Where(item => item.Trashed != null && item.Trashed < cutoff).ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Item>> ActiveFilesAsync(Guid? after, int limit, CancellationToken cancellationToken)
    {
        return await Items
            .AsNoTracking()
            .Where(item => item.Kind == ItemKind.File && item.Trashed == null && (after == null || item.Id.CompareTo(after.Value) > 0))
            .OrderBy(item => item.Id)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }

    public Task<long> UsedBytesAsync(string ownerId, CancellationToken cancellationToken)
    {
        return Items.Where(item => item.OwnerId == ownerId).SumAsync(item => item.Size, cancellationToken);
    }

    public async Task<IReadOnlyList<string>> StorageKeysAsync(string ownerId, CancellationToken cancellationToken)
    {
        List<string?> keys = await Items.Where(item => item.OwnerId == ownerId).Select(item => item.StorageKey).ToListAsync(cancellationToken);
        return keys.OfType<string>().ToList();
    }

    public void Add(Item item)
    {
        Items.Add(item);
    }

    public void Remove(Item item)
    {
        Items.Remove(item);
    }

    public Task SaveAsync(CancellationToken cancellationToken)
    {
        return context.SaveChangesAsync(cancellationToken);
    }
}
