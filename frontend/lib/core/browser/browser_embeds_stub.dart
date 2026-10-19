import 'package:flutter/widgets.dart';

import 'browser_embeds.dart';

BrowserEmbeds createBrowserEmbeds() => const _UnsupportedBrowserEmbeds();

class _UnsupportedBrowserEmbeds implements BrowserEmbeds {
  const _UnsupportedBrowserEmbeds();

  @override
  Widget video(Uri source) => const SizedBox.shrink();

  @override
  Widget audio(Uri source) => const SizedBox.shrink();

  @override
  Widget document(Uri source) => const SizedBox.shrink();
}
