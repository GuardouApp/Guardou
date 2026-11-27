using Guardou.Domain.Notifications;

namespace Guardou.Application.Notifications;

public interface INotificationStore
{
    Task<IReadOnlyList<NotificationRecord>> ListAsync(string userId, int limit, CancellationToken cancellationToken);

    Task<bool> HasUpdateAsync(string userId, string version, CancellationToken cancellationToken);

    Task MarkAllReadAsync(string userId, CancellationToken cancellationToken);

    Task DeleteAllAsync(string userId, CancellationToken cancellationToken);

    void Add(Notification notification);

    Task SaveAsync(CancellationToken cancellationToken);
}
