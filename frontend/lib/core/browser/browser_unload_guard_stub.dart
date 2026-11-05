import 'browser_unload_guard.dart';

BrowserUnloadGuard createBrowserUnloadGuard() => const _UnsupportedBrowserUnloadGuard();

class _UnsupportedBrowserUnloadGuard implements BrowserUnloadGuard {
  const _UnsupportedBrowserUnloadGuard();

  @override
  void warn({required bool enabled}) {}
}
