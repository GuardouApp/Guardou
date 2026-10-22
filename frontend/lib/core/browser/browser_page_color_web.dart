import 'package:flutter/painting.dart';
import 'package:web/web.dart' as web;

import 'browser_page_color.dart';

BrowserPageColor createBrowserPageColor() => const _WebBrowserPageColor();

class _WebBrowserPageColor implements BrowserPageColor {
  const _WebBrowserPageColor();

  static const _themeColorSelector = 'meta[name="theme-color"]';

  @override
  void paint(Color color) {
    final css = _cssOf(color);
    web.document.body?.style.backgroundColor = css;
    web.document.querySelector(_themeColorSelector)?.setAttribute('content', css);
  }

  String _cssOf(Color color) => '#${(color.toARGB32() & 0xFFFFFF).toRadixString(16).padLeft(6, '0')}';
}
