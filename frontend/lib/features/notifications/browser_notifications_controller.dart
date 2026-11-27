import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../core/browser/browser_notifications.dart';
import '../../core/browser/local_store.dart';

final browserNotificationsControllerProvider = NotifierProvider<BrowserNotificationsController, bool>(
  BrowserNotificationsController.new,
);

class BrowserNotificationsController extends Notifier<bool> {
  static const storageKey = 'guardou.browserNotifications';

  BrowserNotifications get _notifications => ref.read(browserNotificationsProvider);

  BrowserNotificationPermission get permission => _notifications.permission;

  @override
  bool build() =>
      ref.read(localStoreProvider).read(storageKey) == '${true}' && permission == BrowserNotificationPermission.granted;

  Future<BrowserNotificationPermission> enable() async {
    final permission = await _notifications.requestPermission();
    _save(permission == BrowserNotificationPermission.granted);
    return permission;
  }

  void disable() => _save(false);

  void _save(bool enabled) {
    ref.read(localStoreProvider).write(storageKey, '$enabled');
    state = enabled;
  }
}
