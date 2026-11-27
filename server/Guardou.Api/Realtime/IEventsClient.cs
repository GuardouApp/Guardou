using Guardou.Application.Notifications;

namespace Guardou.Api.Realtime;

public interface IEventsClient
{
    Task ItemsChanged(ItemsChangedEvent change);

    Task SharesChanged();

    Task NotificationReceived(NotificationEntry notification);
}
