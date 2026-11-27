using Guardou.Application.Files;

namespace Guardou.Infrastructure.Persistence;

internal sealed class ItemChangeOutbox(AppDbContext context, IItemChangeObserver observer)
{
    private readonly List<ItemChanges> _pending = [];

    public async Task AddAsync(ItemChanges changes, CancellationToken cancellationToken)
    {
        _pending.Add(changes);
        if (context.Database.CurrentTransaction is null)
        {
            await FlushAsync(cancellationToken);
        }
    }

    public async Task FlushAsync(CancellationToken cancellationToken)
    {
        ItemChanges[] pending = [.. _pending];
        _pending.Clear();
        foreach (ItemChanges changes in pending)
        {
            await observer.ChangedAsync(changes, cancellationToken);
        }
    }
}
