import 'package:dio/dio.dart';
import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:guardou/api/generated/models/authentication_method.dart';
import 'package:guardou/core/api/api_problem.dart';
import 'package:guardou/features/auth/login_page.dart';
import 'package:mocktail/mocktail.dart';

import '../../helpers/test_app.dart';

void main() {
  late MockAuthService auth;

  setUp(() => auth = MockAuthService());

  Future<void> pumpLogin(
    WidgetTester tester, {
    Size size = const Size(1280, 800),
    List<AuthenticationMethod> methods = const [AuthenticationMethod.password],
    MockBrowserLocation? location,
    String? redirect,
    String? oidcError,
    bool signupOpen = false,
  }) => pumpTestApp(
    tester,
    size: size,
    overrides: serviceOverrides(
      auth: auth,
      user: null,
      configuration: testConfiguration.copyWith(authenticationMethods: methods, signupOpen: signupOpen),
      location: location,
    ),
    child: LoginPage(redirect: redirect, oidcError: oidcError),
  );

  Future<void> submit(WidgetTester tester) async {
    await tester.enterText(settingsField('Username'), 'admin');
    await tester.enterText(settingsField('Password'), 'secret-password');
    await tester.tap(find.text('Sign in'));
    await tester.pumpAndSettle();
  }

  void loginFailsWith(int status, {List<String> codes = const []}) {
    final options = RequestOptions(path: '/api/auth/login');
    when(
      () => auth.login(
        username: any(named: 'username'),
        password: any(named: 'password'),
        rememberMe: false,
      ),
    ).thenThrow(
      DioException(
        requestOptions: options,
        response: Response<Object?>(requestOptions: options, statusCode: status),
        error: ApiProblem(status: status, codes: codes),
      ),
    );
  }

  testWidgets('shows the application name', (tester) async {
    await pumpLogin(tester);

    expect(find.text('Sign in to Guardou'), findsOneWidget);
  });

  testWidgets('requires a username and a password', (tester) async {
    await pumpLogin(tester);

    await tester.tap(find.text('Sign in'));
    await tester.pump();

    expect(find.text('Enter a value.'), findsNWidgets(2));
    verifyNever(
      () => auth.login(
        username: any(named: 'username'),
        password: any(named: 'password'),
        rememberMe: any(named: 'rememberMe'),
      ),
    );
  });

  testWidgets('shows a message for wrong credentials', (tester) async {
    loginFailsWith(401);
    await pumpLogin(tester);

    await submit(tester);

    expect(find.text('The username or the password is wrong.'), findsOneWidget);
  });

  testWidgets('shows a generic message for other failures in portrait', (tester) async {
    loginFailsWith(500);
    await pumpLogin(tester, size: const Size(390, 844));

    await submit(tester);

    expect(find.text('Something went wrong. Try again.'), findsOneWidget);
  });

  testWidgets('signs in with the given credentials', (tester) async {
    when(
      () => auth.login(username: 'admin', password: 'secret-password', rememberMe: false),
    ).thenAnswer((_) async => testUser);
    await pumpLogin(tester);

    await submit(tester);

    verify(() => auth.login(username: 'admin', password: 'secret-password', rememberMe: false)).called(1);
    expect(find.text('The username or the password is wrong.'), findsNothing);
  });

  testWidgets('signs in with single sign-on and keeps the target', (tester) async {
    final location = MockBrowserLocation();
    final oidcLogin = Uri.parse('/api/auth/oidc/login?returnUrl=%2Ffiles');
    when(() => auth.oidcLogin(returnUrl: '/files')).thenReturn(oidcLogin);
    await pumpLogin(
      tester,
      methods: const [AuthenticationMethod.password, AuthenticationMethod.oidc],
      location: location,
      redirect: '/files',
    );

    await tester.tap(find.text('Sign in with single sign-on'));
    await tester.pump();

    verify(() => location.assign(oidcLogin)).called(1);
    expect(find.text('or'), findsOneWidget);
  });

  testWidgets('shows only single sign-on when the password login is disabled in portrait', (tester) async {
    await pumpLogin(tester, size: const Size(390, 844), methods: const [AuthenticationMethod.oidc]);

    expect(find.widgetWithText(FilledButton, 'Sign in with single sign-on'), findsOneWidget);
    expect(settingsField('Username'), findsNothing);
    expect(find.text('or'), findsNothing);
  });

  testWidgets('shows the reason of a refused single sign-on', (tester) async {
    await pumpLogin(tester, methods: const [AuthenticationMethod.oidc], oidcError: 'noAccount');

    expect(find.text('No user is linked to your account. Ask an administrator to create one.'), findsOneWidget);
  });

  testWidgets('shows a message when the password login is disabled', (tester) async {
    loginFailsWith(403);
    await pumpLogin(tester);

    await submit(tester);

    expect(find.text('The password login is disabled. Use single sign-on.'), findsOneWidget);
  });

  testWidgets('shows a message when the account is locked', (tester) async {
    loginFailsWith(403, codes: const ['lockedOut']);
    await pumpLogin(tester);

    await submit(tester);

    expect(
      find.text('Too many failed sign-ins. The account is locked for some minutes. Try again later.'),
      findsOneWidget,
    );
  });

  testWidgets('shows a message when the login is rate limited', (tester) async {
    loginFailsWith(429);
    await pumpLogin(tester);

    await submit(tester);

    expect(find.text('Too many attempts. Wait one minute, then try again.'), findsOneWidget);
  });

  testWidgets('hides the sign-up link while the sign-up is closed', (tester) async {
    await pumpLogin(tester);

    expect(find.text('Create an account'), findsNothing);
  });

  testWidgets('shows the sign-up link while the sign-up is open in portrait', (tester) async {
    await pumpLogin(tester, size: const Size(390, 844), signupOpen: true);

    expect(find.text('Create an account'), findsOneWidget);
  });
}
