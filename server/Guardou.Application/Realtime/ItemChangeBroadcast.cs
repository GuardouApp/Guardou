using Guardou.Application.Files;
using Guardou.Application.Quotas;
using Guardou.Domain.Files;

namespace Guardou.Application.Realtime;

internal sealed class ItemChangeBroadcast(IRealtimeEvents events, QuotaWarnings quotas) : IItemChangeObserver
{
    public async Task ChangedAsync(ItemChanges changes, CancellationToken cancellationToken)
    {
        foreach (IGrouping<string, ChangedFolder> owner in changes.Folders.GroupBy(folder => folder.OwnerId))
        {
            List<string> folderIds = owner.Select(folder => ItemId.Format(folder.FolderId)).Distinct().ToList();
            await events.ItemsChangedAsync(owner.Key, folderIds, cancellationToken);
        }

        foreach ((string ownerId, long bytes) in changes.UsageChanges)
        {
            await quotas.UsageChangedAsync(ownerId, bytes, cancellationToken);
        }
    }
}
