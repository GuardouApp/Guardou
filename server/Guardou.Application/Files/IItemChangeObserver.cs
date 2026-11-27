namespace Guardou.Application.Files;

public interface IItemChangeObserver
{
    Task ChangedAsync(ItemChanges changes, CancellationToken cancellationToken);
}
