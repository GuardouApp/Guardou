import 'dart:async';

import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../core/realtime/realtime_connection.dart';
import '../../core/realtime/realtime_event.dart';
import '../auth/session_controller.dart';
import '../files/file_views.dart';
import '../notifications/notifications_controller.dart';
import '../preferences/preferences_controller.dart';
import '../shares/shared_with_me.dart';
import '../users/users_controller.dart';
import 'item_changes.dart';

final realtimeControllerProvider = NotifierProvider<RealtimeController, RealtimeStatus>(RealtimeController.new);

class RealtimeController extends Notifier<RealtimeStatus> {
  @override
  RealtimeStatus build() {
    final userId = ref.watch(
      sessionControllerProvider.select(
        (session) => switch (session.value) {
          final user? when !user.passwordChangeRequired => user.id,
          _ => null,
        },
      ),
    );
    if (userId == null) {
      return RealtimeStatus.disconnected;
    }
    final connection = ref.read(realtimeConnectionFactoryProvider)();
    final events = connection.events.listen(_apply);
    final statuses = connection.statuses.listen(_changeStatus);
    ref.onDispose(() {
      unawaited(events.cancel());
      unawaited(statuses.cancel());
      unawaited(connection.stop());
    });
    unawaited(connection.start());
    return RealtimeStatus.connecting;
  }

  void _changeStatus(RealtimeStatus next) {
    if (next == RealtimeStatus.connected && state == RealtimeStatus.reconnecting) {
      _refreshAll();
    }
    state = next;
  }

  void _apply(RealtimeEvent event) {
    switch (event) {
      case ItemsChanged():
        ref.read(itemChangesProvider.notifier).publish(event);
        _refreshLists();
      case SharesChanged():
        ref.invalidate(sharedWithMeProvider);
      case NotificationReceived(:final notification):
        ref.read(notificationsControllerProvider.notifier).receive(notification);
    }
  }

  void _refreshLists() {
    for (final provider in listViews) {
      ref.invalidate(provider);
    }
  }

  void _refreshAll() {
    refreshFileViews(ref.invalidate);
    unawaited(ref.read(sessionControllerProvider.notifier).refresh());
    ref
      ..invalidate(preferencesControllerProvider)
      ..invalidate(usersControllerProvider)
      ..invalidate(notificationsControllerProvider);
  }
}
