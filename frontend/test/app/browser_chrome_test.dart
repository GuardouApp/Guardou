import 'dart:async';

import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:guardou/app/browser_chrome.dart';
import 'package:guardou/app/theme/app_theme.dart';
import 'package:guardou/core/browser/browser_page_color.dart';
import 'package:guardou/core/browser/browser_safe_area.dart';

class _FakeSafeArea implements BrowserSafeArea {
  final _changes = StreamController<void>.broadcast();

  @override
  EdgeInsets insets = const EdgeInsets.only(top: 47, bottom: 34);

  @override
  Stream<void> get changes => _changes.stream;

  void change(EdgeInsets value) {
    insets = value;
    _changes.add(null);
  }
}

class _FakePageColor implements BrowserPageColor {
  final painted = <Color>[];

  @override
  void paint(Color color) => painted.add(color);
}

void main() {
  late _FakeSafeArea safeArea;
  late _FakePageColor pageColor;

  setUp(() {
    safeArea = _FakeSafeArea();
    pageColor = _FakePageColor();
  });

  Future<MediaQueryData Function()> pump(WidgetTester tester, {EdgeInsets keyboard = EdgeInsets.zero}) async {
    late BuildContext inner;
    await tester.pumpWidget(
      ProviderScope(
        overrides: [
          browserSafeAreaProvider.overrideWithValue(safeArea),
          browserPageColorProvider.overrideWithValue(pageColor),
        ],
        child: MaterialApp(
          theme: AppTheme.dark(),
          builder: (context, child) => MediaQuery(
            data: MediaQuery.of(context).copyWith(viewInsets: keyboard),
            child: BrowserChrome(child: child ?? const SizedBox.shrink()),
          ),
          home: Builder(
            builder: (context) {
              inner = context;
              return const SizedBox.shrink();
            },
          ),
        ),
      ),
    );
    return () => MediaQuery.of(inner);
  }

  testWidgets('adds the browser safe area to the padding', (tester) async {
    final media = await pump(tester);

    expect(media().viewPadding, const EdgeInsets.only(top: 47, bottom: 34));
    expect(media().padding, const EdgeInsets.only(top: 47, bottom: 34));
  });

  testWidgets('removes the bottom padding under the keyboard', (tester) async {
    final media = await pump(tester, keyboard: const EdgeInsets.only(bottom: 300));

    expect(media().padding, const EdgeInsets.only(top: 47));
  });

  testWidgets('follows safe area changes', (tester) async {
    final media = await pump(tester);

    safeArea.change(const EdgeInsets.only(left: 59, right: 59));
    await tester.pump();

    expect(media().viewPadding, const EdgeInsets.only(left: 59, right: 59));
  });

  testWidgets('paints the page with the scaffold color', (tester) async {
    await pump(tester);

    expect(pageColor.painted.last, AppTheme.dark().scaffoldBackgroundColor);
  });
}
