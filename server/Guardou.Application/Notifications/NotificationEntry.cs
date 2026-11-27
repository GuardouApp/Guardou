using Guardou.Application.Users;
using Guardou.Domain.Notifications;

namespace Guardou.Application.Notifications;

public sealed record NotificationEntry(
    string Id,
    NotificationKind Kind,
    DateTimeOffset Created,
    bool Read,
    NotificationItem? Item,
    UserSummary? Sender,
    string? Version,
    string? Url);
