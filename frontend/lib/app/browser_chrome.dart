import 'dart:async';
import 'dart:math';

import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../core/browser/browser_page_color.dart';
import '../core/browser/browser_safe_area.dart';

class BrowserChrome extends ConsumerStatefulWidget {
  const BrowserChrome({super.key, required this.child});

  final Widget child;

  @override
  ConsumerState<BrowserChrome> createState() => _BrowserChromeState();
}

class _BrowserChromeState extends ConsumerState<BrowserChrome> {
  late final StreamSubscription<void> _safeAreaChanges;

  @override
  void initState() {
    super.initState();
    _safeAreaChanges = ref.read(browserSafeAreaProvider).changes.listen((_) => setState(() {}));
  }

  @override
  void didChangeDependencies() {
    super.didChangeDependencies();
    ref.read(browserPageColorProvider).paint(Theme.of(context).scaffoldBackgroundColor);
  }

  @override
  void dispose() {
    unawaited(_safeAreaChanges.cancel());
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    final media = MediaQuery.of(context);
    final viewPadding = _larger(media.viewPadding, ref.read(browserSafeAreaProvider).insets);
    return MediaQuery(
      data: media.copyWith(viewPadding: viewPadding, padding: _withoutKeyboard(viewPadding, media.viewInsets)),
      child: widget.child,
    );
  }

  EdgeInsets _larger(EdgeInsets first, EdgeInsets second) => EdgeInsets.fromLTRB(
    max(first.left, second.left),
    max(first.top, second.top),
    max(first.right, second.right),
    max(first.bottom, second.bottom),
  );

  EdgeInsets _withoutKeyboard(EdgeInsets viewPadding, EdgeInsets viewInsets) =>
      viewPadding.copyWith(bottom: max(0, viewPadding.bottom - viewInsets.bottom));
}
