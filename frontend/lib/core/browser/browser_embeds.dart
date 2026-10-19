import 'package:flutter/widgets.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import 'browser_embeds_stub.dart' if (dart.library.js_interop) 'browser_embeds_web.dart';

abstract interface class BrowserEmbeds {
  Widget video(Uri source);

  Widget audio(Uri source);

  Widget document(Uri source);
}

final browserEmbedsProvider = Provider<BrowserEmbeds>((ref) => createBrowserEmbeds());
