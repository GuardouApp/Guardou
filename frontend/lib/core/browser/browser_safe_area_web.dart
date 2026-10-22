import 'package:flutter/painting.dart';
import 'package:web/web.dart' as web;

import 'browser_safe_area.dart';

BrowserSafeArea createBrowserSafeArea() => _WebBrowserSafeArea(_createProbe());

const _safeAreaPadding =
    'env(safe-area-inset-top) env(safe-area-inset-right) env(safe-area-inset-bottom) env(safe-area-inset-left)';

web.HTMLElement _createProbe() {
  final probe = web.HTMLDivElement();
  probe.style
    ..position = 'fixed'
    ..visibility = 'hidden'
    ..pointerEvents = 'none'
    ..padding = _safeAreaPadding;
  web.document.body?.append(probe);
  return probe;
}

class _WebBrowserSafeArea implements BrowserSafeArea {
  _WebBrowserSafeArea(this._probe);

  final web.HTMLElement _probe;

  @override
  EdgeInsets get insets {
    final style = web.window.getComputedStyle(_probe);
    return EdgeInsets.fromLTRB(
      _pixels(style.paddingLeft),
      _pixels(style.paddingTop),
      _pixels(style.paddingRight),
      _pixels(style.paddingBottom),
    );
  }

  @override
  Stream<void> get changes => web.EventStreamProviders.resizeEvent.forTarget(web.window);

  double _pixels(String value) => double.tryParse(value.replaceAll('px', '')) ?? 0;
}
