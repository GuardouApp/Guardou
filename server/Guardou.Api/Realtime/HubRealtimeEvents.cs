using Guardou.Application.Notifications;
using Guardou.Application.Realtime;
using Microsoft.AspNetCore.SignalR;

namespace Guardou.Api.Realtime;

internal sealed class HubRealtimeEvents(IHubContext<EventsHub, IEventsClient> hub) : IRealtimeEvents
{
    public Task ItemsChangedAsync(string userId, IReadOnlyCollection<string> folderIds, CancellationToken cancellationToken)
    {
        return User(userId).ItemsChanged(new ItemsChangedEvent(folderIds));
    }

    public Task SharesChangedAsync(string userId, CancellationToken cancellationToken)
    {
        return User(userId).SharesChanged();
    }

    public Task NotificationReceivedAsync(string userId, NotificationEntry notification, CancellationToken cancellationToken)
    {
        return User(userId).NotificationReceived(notification);
    }

    private IEventsClient User(string userId)
    {
        return hub.Clients.Group(EventsHub.UserGroup(userId));
    }
}
