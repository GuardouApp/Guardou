import 'dart:async';

import 'package:web/web.dart' as web;

import 'browser_unload_guard.dart';

BrowserUnloadGuard createBrowserUnloadGuard() => _WebBrowserUnloadGuard();

class _WebBrowserUnloadGuard implements BrowserUnloadGuard {
  StreamSubscription<web.BeforeUnloadEvent>? _subscription;

  @override
  void warn({required bool enabled}) {
    if (!enabled) {
      unawaited(_subscription?.cancel());
      _subscription = null;
      return;
    }
    _subscription ??= web.EventStreamProviders.beforeUnloadEvent.forTarget(web.window).listen((event) {
      event
        ..preventDefault()
        ..returnValue = '';
    });
  }
}
