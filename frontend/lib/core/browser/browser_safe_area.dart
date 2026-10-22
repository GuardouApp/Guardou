import 'package:flutter/painting.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import 'browser_safe_area_stub.dart' if (dart.library.js_interop) 'browser_safe_area_web.dart';

abstract interface class BrowserSafeArea {
  EdgeInsets get insets;

  Stream<void> get changes;
}

final browserSafeAreaProvider = Provider<BrowserSafeArea>((ref) => createBrowserSafeArea());
