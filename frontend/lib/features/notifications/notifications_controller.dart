import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../api/generated/models/notification_entry.dart';
import '../auth/session_controller.dart';
import 'notifications_service.dart';

final notificationsControllerProvider = AsyncNotifierProvider<NotificationsController, List<NotificationEntry>>(
  NotificationsController.new,
);

final unreadNotificationsProvider = Provider<int>(
  (ref) => ref.watch(notificationsControllerProvider).value?.where((notification) => !notification.read).length ?? 0,
);

final incomingNotificationProvider = NotifierProvider<IncomingNotification, NotificationEntry?>(
  IncomingNotification.new,
);

class NotificationsController extends AsyncNotifier<List<NotificationEntry>> {
  @override
  Future<List<NotificationEntry>> build() async {
    final userId = ref.watch(sessionControllerProvider.select((session) => session.value?.id));
    return userId == null ? const [] : ref.read(notificationsServiceProvider).list();
  }

  void receive(NotificationEntry notification) {
    final current = state.value ?? const <NotificationEntry>[];
    state = AsyncData([notification, ...current.where((entry) => entry.id != notification.id)]);
    ref.read(incomingNotificationProvider.notifier).show(notification);
  }

  Future<void> markAllRead() async {
    final current = state.value ?? const <NotificationEntry>[];
    if (current.every((notification) => notification.read)) {
      return;
    }
    await ref.read(notificationsServiceProvider).markAllRead();
    state = AsyncData([for (final notification in current) notification.copyWith(read: true)]);
  }

  Future<void> clear() async {
    await ref.read(notificationsServiceProvider).clear();
    state = const AsyncData([]);
  }
}

class IncomingNotification extends Notifier<NotificationEntry?> {
  @override
  NotificationEntry? build() => null;

  void show(NotificationEntry notification) => state = notification;
}
