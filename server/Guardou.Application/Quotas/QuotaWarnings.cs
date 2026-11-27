using Guardou.Application.Notifications;
using Guardou.Domain.Users;

namespace Guardou.Application.Quotas;

public sealed class QuotaWarnings(IStorageQuotaStore quotas, NotificationCenter notifications)
{
    public async Task UsageChangedAsync(string ownerId, long bytes, CancellationToken cancellationToken)
    {
        if (bytes > 0)
        {
            StorageQuota quota = await quotas.FindAsync(ownerId, cancellationToken);
            await WarnAsync(ownerId, quota, quota with { UsedBytes = quota.UsedBytes - bytes }, cancellationToken);
        }
    }

    public async Task LimitChangedAsync(string ownerId, long? previousLimitBytes, CancellationToken cancellationToken)
    {
        StorageQuota quota = await quotas.FindAsync(ownerId, cancellationToken);
        await WarnAsync(ownerId, quota, quota with { LimitBytes = previousLimitBytes }, cancellationToken);
    }

    private async Task WarnAsync(string ownerId, StorageQuota current, StorageQuota previous, CancellationToken cancellationToken)
    {
        if (current.ReachedWarningSince(previous))
        {
            await notifications.QuotaWarningAsync(ownerId, cancellationToken);
        }
    }
}
