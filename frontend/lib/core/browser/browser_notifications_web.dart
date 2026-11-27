import 'dart:js_interop';
import 'dart:js_interop_unsafe';

import 'package:flutter/foundation.dart';
import 'package:web/web.dart' as web;

import 'browser_notifications.dart';

BrowserNotifications createBrowserNotifications() => const _WebBrowserNotifications();

class _WebBrowserNotifications implements BrowserNotifications {
  const _WebBrowserNotifications();

  static const _icon = 'icons/Icon-192.png';
  static const _visible = 'visible';

  bool get _supported => globalContext.has('Notification');

  @override
  BrowserNotificationPermission get permission =>
      _supported ? _permissionOf(web.Notification.permission) : BrowserNotificationPermission.unsupported;

  @override
  bool get pageVisible => web.document.visibilityState == _visible;

  @override
  Future<BrowserNotificationPermission> requestPermission() async {
    if (!_supported) {
      return BrowserNotificationPermission.unsupported;
    }
    return _permissionOf((await web.Notification.requestPermission().toDart).toDart);
  }

  @override
  void show({required String title, required String body, required VoidCallback onClick}) {
    if (permission != BrowserNotificationPermission.granted) {
      return;
    }
    final notification = web.Notification(title, web.NotificationOptions(body: body, icon: _icon));
    notification.onclick = ((web.Event _) {
      web.window.focus();
      notification.close();
      onClick();
    }).toJS;
  }

  static BrowserNotificationPermission _permissionOf(String value) => switch (value) {
    'granted' => BrowserNotificationPermission.granted,
    'denied' => BrowserNotificationPermission.denied,
    _ => BrowserNotificationPermission.prompt,
  };
}
