import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:guardou/features/shell/account_menu.dart';
import 'package:mocktail/mocktail.dart';

import '../../helpers/test_app.dart';

void main() {
  late MockAuthService auth;

  setUp(() => auth = MockAuthService());

  Future<void> openPanel(WidgetTester tester, Size size) async {
    await pumpTestApp(
      tester,
      size: size,
      overrides: serviceOverrides(auth: auth),
      child: const Scaffold(
        body: Align(alignment: Alignment.topRight, child: AccountMenu()),
      ),
    );
    await tester.tap(find.byTooltip('Account'));
    await tester.pumpAndSettle();
  }

  for (final size in [const Size(1280, 800), const Size(390, 844)]) {
    testWidgets('shows the account panel with a margin at $size', (tester) async {
      await openPanel(tester, size);

      final panel = tester.getRect(find.byType(AccountPanel));
      expect(find.text('admin@example.com'), findsOneWidget);
      expect(find.text('Account settings'), findsOneWidget);
      expect(panel.right, lessThanOrEqualTo(size.width - 8));
      expect(panel.left, greaterThanOrEqualTo(8));
    });
  }

  testWidgets('signs out and closes the panel', (tester) async {
    when(auth.logout).thenAnswer((_) async {});
    await openPanel(tester, const Size(1280, 800));

    await tester.tap(find.text('Sign out'));
    await tester.pumpAndSettle();

    verify(auth.logout).called(1);
    expect(find.byType(AccountPanel), findsNothing);
  });
}
