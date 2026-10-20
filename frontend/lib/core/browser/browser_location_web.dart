import 'package:web/web.dart' as web;

import 'browser_location.dart';

BrowserLocation createBrowserLocation() => const _WebBrowserLocation();

class _WebBrowserLocation implements BrowserLocation {
  const _WebBrowserLocation();

  @override
  void assign(Uri uri) => web.window.location.assign(uri.toString());
}
