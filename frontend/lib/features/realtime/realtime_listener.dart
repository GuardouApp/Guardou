import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../api/generated/models/notification_entry.dart';
import '../../core/browser/browser_notifications.dart';
import '../../core/realtime/realtime_connection.dart';
import '../../l10n/generated/app_localizations.dart';
import '../notifications/browser_notifications_controller.dart';
import '../notifications/notification_opener.dart';
import '../notifications/notification_presentation.dart';
import '../notifications/notifications_controller.dart';
import 'realtime_controller.dart';

class RealtimeListener extends ConsumerWidget {
  const RealtimeListener({super.key, required this.child});

  static const _connectionLostDuration = Duration(days: 1);

  final Widget child;

  void _showStatus(BuildContext context, RealtimeStatus? previous, RealtimeStatus next) {
    final l10n = AppLocalizations.of(context);
    final messenger = ScaffoldMessenger.of(context);
    if (next == RealtimeStatus.reconnecting && previous == RealtimeStatus.connected) {
      messenger
        ..hideCurrentSnackBar()
        ..showSnackBar(SnackBar(content: Text(l10n.realtimeReconnecting), duration: _connectionLostDuration));
    } else if (next == RealtimeStatus.connected && previous == RealtimeStatus.reconnecting) {
      messenger
        ..hideCurrentSnackBar()
        ..showSnackBar(SnackBar(content: Text(l10n.realtimeReconnected)));
    }
  }

  void _showNotification(BuildContext context, WidgetRef ref, NotificationEntry notification) {
    final l10n = AppLocalizations.of(context);
    final message = notification.message(l10n);
    final target = notification.target;
    final opener = ref.read(notificationOpenerProvider);
    final browser = ref.read(browserNotificationsProvider);
    if (ref.read(browserNotificationsControllerProvider) && !browser.pageVisible) {
      browser.show(
        title: l10n.notifications,
        body: message,
        onClick: () {
          if (target != null) {
            opener.open(target);
          }
        },
      );
      return;
    }
    ScaffoldMessenger.of(context).showSnackBar(
      SnackBar(
        content: Text(message),
        action: target == null ? null : SnackBarAction(label: l10n.notificationOpen, onPressed: () => opener.open(target)),
      ),
    );
  }

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    ref
      ..listen(realtimeControllerProvider, (previous, next) => _showStatus(context, previous, next))
      ..listen(incomingNotificationProvider, (_, next) {
        if (next != null) {
          _showNotification(context, ref, next);
        }
      });
    return child;
  }
}
