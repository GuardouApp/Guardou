import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../api/generated/api_client.dart';
import '../../api/generated/models/notification_entry.dart';
import '../../core/api/api_providers.dart';

final notificationsServiceProvider = Provider<NotificationsService>(
  (ref) => NotificationsService(ref.watch(apiClientProvider)),
);

class NotificationsService {
  const NotificationsService(this._api);

  final ApiClient _api;

  Future<List<NotificationEntry>> list() => _api.notifications.listNotifications();

  Future<void> markAllRead() => _api.notifications.markNotificationsRead();

  Future<void> clear() => _api.notifications.clearNotifications();
}
