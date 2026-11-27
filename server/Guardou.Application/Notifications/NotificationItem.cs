using Guardou.Domain.Files;

namespace Guardou.Application.Notifications;

public sealed record NotificationItem(string Id, string Name, ItemKind Kind);
