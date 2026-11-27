import '../../api/generated/models/notification_entry.dart';

sealed class RealtimeEvent {
  const RealtimeEvent();
}

final class ItemsChanged extends RealtimeEvent {
  const ItemsChanged(this.folderIds);

  ItemsChanged.fromJson(Map<String, Object?> json) : folderIds = {...(json['folderIds'] as List<Object?>).cast<String>()};

  final Set<String> folderIds;

  bool touchesAny(Iterable<String?> ids) => ids.any(folderIds.contains);
}

final class SharesChanged extends RealtimeEvent {
  const SharesChanged();
}

final class NotificationReceived extends RealtimeEvent {
  const NotificationReceived(this.notification);

  NotificationReceived.fromJson(Map<String, Object?> json) : notification = NotificationEntry.fromJson(json);

  final NotificationEntry notification;
}
