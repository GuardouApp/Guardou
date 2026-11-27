using Guardou.Application.Realtime;
using Guardou.Application.Users;
using Guardou.Domain;
using Guardou.Domain.Files;
using Guardou.Domain.Notifications;

namespace Guardou.Application.Notifications;

public sealed class NotificationCenter(INotificationStore store, IUserDirectory users, IRealtimeEvents events, TimeProvider time)
{
    private const int HistoryLimit = 100;

    public async Task<IReadOnlyList<NotificationEntry>> ListAsync(string userId, CancellationToken cancellationToken)
    {
        IReadOnlyList<NotificationRecord> records = await store.ListAsync(userId, HistoryLimit, cancellationToken);
        List<string> senderIds = records.Select(record => record.Notification.SenderId).OfType<string>().Distinct().ToList();
        Dictionary<string, UserSummary> senders = (await users.FindAsync(senderIds, cancellationToken)).ToDictionary(user => user.Id);
        return records.Select(record => EntryOf(record, record.Notification.SenderId is { } senderId ? senders.GetValueOrDefault(senderId) : null)).ToList();
    }

    public Task MarkAllReadAsync(string userId, CancellationToken cancellationToken)
    {
        return store.MarkAllReadAsync(userId, cancellationToken);
    }

    public Task ClearAsync(string userId, CancellationToken cancellationToken)
    {
        return store.DeleteAllAsync(userId, cancellationToken);
    }

    public async Task ShareReceivedAsync(string recipientId, string senderId, Item item, CancellationToken cancellationToken)
    {
        UserSummary? sender = (await users.FindAsync([senderId], cancellationToken)).SingleOrDefault();
        NotificationItem shared = new(ItemId.Format(item.Id), item.Name, item.Kind);
        await DeliverAsync(Notification.ShareReceived(recipientId, senderId, item.Id, time.GetUtcNow()), shared, sender, cancellationToken);
    }

    public Task QuotaWarningAsync(string userId, CancellationToken cancellationToken)
    {
        return DeliverAsync(Notification.QuotaWarning(userId, time.GetUtcNow()), item: null, sender: null, cancellationToken);
    }

    public async Task UpdateAvailableAsync(IReadOnlyCollection<string> userIds, string version, CancellationToken cancellationToken)
    {
        foreach (string userId in userIds)
        {
            if (!await store.HasUpdateAsync(userId, version, cancellationToken))
            {
                await DeliverAsync(Notification.UpdateAvailable(userId, version, time.GetUtcNow()), item: null, sender: null, cancellationToken);
            }
        }
    }

    private async Task DeliverAsync(Notification notification, NotificationItem? item, UserSummary? sender, CancellationToken cancellationToken)
    {
        store.Add(notification);
        await store.SaveAsync(cancellationToken);
        await events.NotificationReceivedAsync(notification.UserId, EntryOf(new NotificationRecord(notification, item), sender), cancellationToken);
    }

    private static NotificationEntry EntryOf(NotificationRecord record, UserSummary? sender)
    {
        Notification notification = record.Notification;
        return new NotificationEntry(
            notification.Id.ToString(),
            notification.Kind,
            notification.Created,
            notification.Read,
            record.Item,
            sender,
            notification.Version,
            notification.Version is { } version ? Product.ReleaseUrl(version) : null);
    }
}
