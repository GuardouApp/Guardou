import 'package:flutter_riverpod/flutter_riverpod.dart';

import 'browser_unload_guard_stub.dart' if (dart.library.js_interop) 'browser_unload_guard_web.dart';

abstract interface class BrowserUnloadGuard {
  void warn({required bool enabled});
}

final browserUnloadGuardProvider = Provider<BrowserUnloadGuard>((ref) => createBrowserUnloadGuard());
