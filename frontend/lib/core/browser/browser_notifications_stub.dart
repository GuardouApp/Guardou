import 'package:flutter/foundation.dart';

import 'browser_notifications.dart';

BrowserNotifications createBrowserNotifications() => const _UnsupportedBrowserNotifications();

class _UnsupportedBrowserNotifications implements BrowserNotifications {
  const _UnsupportedBrowserNotifications();

  @override
  BrowserNotificationPermission get permission => BrowserNotificationPermission.unsupported;

  @override
  bool get pageVisible => true;

  @override
  Future<BrowserNotificationPermission> requestPermission() async => BrowserNotificationPermission.unsupported;

  @override
  void show({required String title, required String body, required VoidCallback onClick}) {}
}
