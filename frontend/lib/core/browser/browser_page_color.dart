import 'package:flutter/painting.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import 'browser_page_color_stub.dart' if (dart.library.js_interop) 'browser_page_color_web.dart';

abstract interface class BrowserPageColor {
  void paint(Color color);
}

final browserPageColorProvider = Provider<BrowserPageColor>((ref) => createBrowserPageColor());
