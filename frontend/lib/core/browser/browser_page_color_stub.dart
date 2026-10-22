import 'package:flutter/painting.dart';

import 'browser_page_color.dart';

BrowserPageColor createBrowserPageColor() => const _UnsupportedBrowserPageColor();

class _UnsupportedBrowserPageColor implements BrowserPageColor {
  const _UnsupportedBrowserPageColor();

  @override
  void paint(Color color) {}
}
