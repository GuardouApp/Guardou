using Guardou.Application.Notifications;

namespace Guardou.Application.Realtime;

public interface IRealtimeEvents
{
    Task ItemsChangedAsync(string userId, IReadOnlyCollection<string> folderIds, CancellationToken cancellationToken);

    Task SharesChangedAsync(string userId, CancellationToken cancellationToken);

    Task NotificationReceivedAsync(string userId, NotificationEntry notification, CancellationToken cancellationToken);
}
