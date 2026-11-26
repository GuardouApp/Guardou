import 'package:dio/dio.dart';
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:guardou/app/router/app_router.dart';
import 'package:guardou/core/api/api_problem.dart';
import 'package:mocktail/mocktail.dart';

import '../../helpers/test_app.dart';

void main() {
  late MockAuthService auth;

  setUp(() => auth = MockAuthService());

  Future<ProviderContainer> pumpPasswordChange(WidgetTester tester, {Size size = const Size(1280, 800)}) =>
      pumpRoutedApp(
        tester,
        location: '/trash',
        size: size,
        overrides: serviceOverrides(auth: auth, user: testUser.copyWith(passwordChangeRequired: true)),
      );

  String locationOf(ProviderContainer container) =>
      container.read(appRouterProvider).routerDelegate.currentConfiguration.uri.path;

  Future<void> submit(WidgetTester tester, {String current = 'old-password'}) async {
    await tester.enterText(settingsField('Current password'), current);
    await tester.enterText(settingsField('New password'), 'new-long-password');
    await tester.enterText(settingsField('Confirm the password'), 'new-long-password');
    await tester.ensureVisible(find.widgetWithText(FilledButton, 'Change password'));
    await tester.pumpAndSettle();
    await tester.tap(find.widgetWithText(FilledButton, 'Change password'));
    await tester.pumpAndSettle();
  }

  for (final size in [const Size(1280, 800), const Size(390, 844)]) {
    testWidgets('forces the password change before the target at $size', (tester) async {
      final container = await pumpPasswordChange(tester, size: size);

      expect(locationOf(container), '/password');
      expect(find.text('Change your password'), findsOneWidget);
      expect(find.text('An administrator requires a new password before you continue.'), findsOneWidget);
      expect(tester.takeException(), isNull);
    });
  }

  testWidgets('opens the target after the change', (tester) async {
    when(
      () => auth.changePassword(currentPassword: 'old-password', newPassword: 'new-long-password'),
    ).thenAnswer((_) async => testUser);
    final container = await pumpPasswordChange(tester);

    await submit(tester);

    expect(locationOf(container), '/trash');
  });

  testWidgets('shows a message for a wrong current password', (tester) async {
    final options = RequestOptions(path: '/api/users/me/password');
    when(
      () => auth.changePassword(
        currentPassword: any(named: 'currentPassword'),
        newPassword: any(named: 'newPassword'),
      ),
    ).thenThrow(
      DioException(
        requestOptions: options,
        response: Response<Object?>(requestOptions: options, statusCode: 400),
        error: const ApiProblem(status: 400, codes: ['PasswordMismatch']),
      ),
    );
    final container = await pumpPasswordChange(tester);

    await submit(tester, current: 'wrong-password');

    expect(find.text('The current password is wrong.'), findsOneWidget);
    expect(locationOf(container), '/password');
  });
}
