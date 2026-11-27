using Guardou.Application.Notifications;
using Guardou.Domain.Files;
using Guardou.Domain.Notifications;
using Microsoft.EntityFrameworkCore;

namespace Guardou.Infrastructure.Persistence;

internal sealed class EfNotificationStore(AppDbContext context) : INotificationStore
{
    private DbSet<Notification> Notifications => context.Set<Notification>();

    public async Task<IReadOnlyList<NotificationRecord>> ListAsync(string userId, int limit, CancellationToken cancellationToken)
    {
        var rows = await Notifications
            .AsNoTracking()
            .Where(notification => notification.UserId == userId)
            .OrderByDescending(notification => notification.Created)
            .ThenByDescending(notification => notification.Id)
            .Take(limit)
            .Select(notification => new
            {
                Notification = notification,
                Item = context.Set<Item>()
                    .Where(item => item.Id == notification.ItemId)
                    .Select(item => new { item.Id, item.Name, item.Kind })
                    .FirstOrDefault(),
            })
            .ToListAsync(cancellationToken);
        return rows
            .Select(row => new NotificationRecord(
                row.Notification,
                row.Item is { } item ? new NotificationItem(ItemId.Format(item.Id), item.Name, item.Kind) : null))
            .ToList();
    }

    public Task<bool> HasUpdateAsync(string userId, string version, CancellationToken cancellationToken)
    {
        return Notifications.AnyAsync(
            notification => notification.UserId == userId && notification.Kind == NotificationKind.UpdateAvailable && notification.Version == version,
            cancellationToken);
    }

    public Task MarkAllReadAsync(string userId, CancellationToken cancellationToken)
    {
        return Notifications
            .Where(notification => notification.UserId == userId && !notification.Read)
            .ExecuteUpdateAsync(setters => setters.SetProperty(notification => notification.Read, true), cancellationToken);
    }

    public Task DeleteAllAsync(string userId, CancellationToken cancellationToken)
    {
        return Notifications.Where(notification => notification.UserId == userId).ExecuteDeleteAsync(cancellationToken);
    }

    public void Add(Notification notification)
    {
        Notifications.Add(notification);
    }

    public Task SaveAsync(CancellationToken cancellationToken)
    {
        return context.SaveChangesAsync(cancellationToken);
    }
}
