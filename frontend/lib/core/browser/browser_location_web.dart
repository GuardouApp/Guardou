import 'package:web/web.dart' as web;

import 'browser_location.dart';

BrowserLocation createBrowserLocation() => const _WebBrowserLocation();

class _WebBrowserLocation implements BrowserLocation {
  const _WebBrowserLocation();

  static const _newTab = '_blank';
  static const _noOpener = 'noopener';

  @override
  void assign(Uri uri) => web.window.location.assign(uri.toString());

  @override
  void openInNewTab(Uri uri) => web.window.open(uri.toString(), _newTab, _noOpener);
}
