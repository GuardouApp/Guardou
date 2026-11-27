using Guardou.Domain.Notifications;

namespace Guardou.Application.Notifications;

public sealed record NotificationRecord(Notification Notification, NotificationItem? Item);
