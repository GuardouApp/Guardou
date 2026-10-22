import 'package:flutter/painting.dart';

import 'browser_safe_area.dart';

BrowserSafeArea createBrowserSafeArea() => const _UnsupportedBrowserSafeArea();

class _UnsupportedBrowserSafeArea implements BrowserSafeArea {
  const _UnsupportedBrowserSafeArea();

  @override
  EdgeInsets get insets => EdgeInsets.zero;

  @override
  Stream<void> get changes => const Stream.empty();
}
