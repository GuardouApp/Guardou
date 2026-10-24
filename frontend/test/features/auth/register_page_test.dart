import 'package:dio/dio.dart';
import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:guardou/core/widgets/labeled_text_field.dart';
import 'package:guardou/core/api/api_problem.dart';
import 'package:guardou/features/auth/register_page.dart';
import 'package:mocktail/mocktail.dart';

import '../../helpers/test_app.dart';

void main() {
  late MockAuthService auth;

  setUp(() => auth = MockAuthService());

  Future<void> pumpRegister(WidgetTester tester, {Size size = const Size(1280, 800)}) => pumpTestApp(
    tester,
    size: size,
    overrides: serviceOverrides(
      auth: auth,
      user: null,
      configuration: testConfiguration.copyWith(registrationOpen: true),
    ),
    child: const RegisterPage(),
  );

  Future<void> fill(
    WidgetTester tester, {
    String username = 'alice',
    String displayName = 'Alice',
    String email = 'alice@example.com',
    String password = 'long-password',
  }) async {
    await tester.enterText(find.widgetWithText(LabeledTextField, 'Username'), username);
    await tester.enterText(find.widgetWithText(LabeledTextField, 'Display name'), displayName);
    await tester.enterText(find.widgetWithText(LabeledTextField, 'Email'), email);
    await tester.enterText(find.widgetWithText(LabeledTextField, 'Password'), password);
    await tester.enterText(find.widgetWithText(LabeledTextField, 'Confirm the password'), password);
    await tester.ensureVisible(find.text('Create account'));
    await tester.tap(find.text('Create account'));
    await tester.pumpAndSettle();
  }

  testWidgets('explains that the first account becomes the administrator', (tester) async {
    await pumpRegister(tester, size: const Size(390, 844));

    expect(find.text('Create the administrator account'), findsOneWidget);
    expect(find.text('No account exists yet. The first account becomes the administrator.'), findsOneWidget);
  });

  testWidgets('shows password errors only after the field loses focus', (tester) async {
    await pumpRegister(tester);

    await tester.enterText(find.widgetWithText(LabeledTextField, 'Password'), 'short');
    await tester.pump();
    expect(find.text('Use at least 12 characters.'), findsNothing);

    final confirmation = find.descendant(
      of: find.widgetWithText(LabeledTextField, 'Confirm the password'),
      matching: find.byType(TextFormField),
    );
    await tester.ensureVisible(confirmation);
    await tester.tap(confirmation);
    await tester.pump();
    expect(find.text('Use at least 12 characters.'), findsOneWidget);
  });

  testWidgets('validates the username and the password policy', (tester) async {
    await pumpRegister(tester);

    await fill(tester, username: 'john.doe', email: 'john', password: 'short');

    expect(find.text('Use only letters and digits.'), findsOneWidget);
    expect(find.text('Enter a valid email address.'), findsOneWidget);
    expect(find.text('Use at least 12 characters.'), findsOneWidget);
    verifyNever(
      () => auth.register(
        username: any(named: 'username'),
        displayName: any(named: 'displayName'),
        email: any(named: 'email'),
        password: any(named: 'password'),
      ),
    );
  });

  testWidgets('registers the first user', (tester) async {
    when(
      () =>
          auth.register(username: 'alice', displayName: 'Alice', email: 'alice@example.com', password: 'long-password'),
    ).thenAnswer((_) async => testUser);
    await pumpRegister(tester);

    await fill(tester);

    verify(
      () =>
          auth.register(username: 'alice', displayName: 'Alice', email: 'alice@example.com', password: 'long-password'),
    ).called(1);
  });

  testWidgets('shows a message when an account already exists', (tester) async {
    final options = RequestOptions(path: '/api/auth/register');
    when(
      () => auth.register(
        username: any(named: 'username'),
        displayName: any(named: 'displayName'),
        email: any(named: 'email'),
        password: any(named: 'password'),
      ),
    ).thenThrow(
      DioException(
        requestOptions: options,
        response: Response<Object?>(requestOptions: options, statusCode: 403),
        error: const ApiProblem(status: 403),
      ),
    );
    await pumpRegister(tester);

    await fill(tester);

    expect(find.text('An account already exists. Sign in instead.'), findsOneWidget);
  });
}
