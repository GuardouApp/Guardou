import 'package:flutter/foundation.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import 'browser_notifications_stub.dart' if (dart.library.js_interop) 'browser_notifications_web.dart';

enum BrowserNotificationPermission { granted, denied, prompt, unsupported }

abstract interface class BrowserNotifications {
  BrowserNotificationPermission get permission;

  bool get pageVisible;

  Future<BrowserNotificationPermission> requestPermission();

  void show({required String title, required String body, required VoidCallback onClick});
}

final browserNotificationsProvider = Provider<BrowserNotifications>((ref) => createBrowserNotifications());
