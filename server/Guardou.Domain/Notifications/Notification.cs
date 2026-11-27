namespace Guardou.Domain.Notifications;

public sealed class Notification
{
    private Notification(Guid id, string userId, NotificationKind kind, DateTimeOffset created)
    {
        Id = id;
        UserId = userId;
        Kind = kind;
        Created = created;
    }

    public Guid Id { get; private set; }

    public string UserId { get; private set; }

    public NotificationKind Kind { get; private set; }

    public DateTimeOffset Created { get; private set; }

    public bool Read { get; private set; }

    public Guid? ItemId { get; private set; }

    public string? SenderId { get; private set; }

    public string? Version { get; private set; }

    public static Notification ShareReceived(string userId, string senderId, Guid itemId, DateTimeOffset now)
    {
        return new Notification(Guid.CreateVersion7(now), userId, NotificationKind.ShareReceived, now)
        {
            SenderId = senderId,
            ItemId = itemId,
        };
    }

    public static Notification QuotaWarning(string userId, DateTimeOffset now)
    {
        return new Notification(Guid.CreateVersion7(now), userId, NotificationKind.QuotaWarning, now);
    }

    public static Notification UpdateAvailable(string userId, string version, DateTimeOffset now)
    {
        return new Notification(Guid.CreateVersion7(now), userId, NotificationKind.UpdateAvailable, now) { Version = version };
    }
}
