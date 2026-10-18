import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:guardou/api/generated/models/current_user.dart';
import 'package:guardou/api/generated/models/user_account.dart';
import 'package:guardou/api/generated/models/user_request.dart';
import 'package:guardou/app/router/app_router.dart';
import 'package:mocktail/mocktail.dart';

import '../../helpers/test_app.dart';

void main() {
  const admin = UserAccount(id: 'admin-id', username: 'admin', email: 'admin@example.com', isAdmin: true);
  const bob = UserAccount(id: 'bob-id', username: 'bob', email: 'bob@example.com', isAdmin: false);
  const compact = Size(390, 844);

  late MockUsersService users;

  setUpAll(() => registerFallbackValue(const UserRequest(username: '', email: '', password: null, isAdmin: false)));

  setUp(() {
    users = MockUsersService();
    when(users.list).thenAnswer((_) async => [admin, bob]);
  });

  Future<ProviderContainer> pumpUsers(
    WidgetTester tester, {
    String location = '/admin/users',
    Size size = const Size(1280, 800),
    CurrentUser? user = testUser,
  }) => pumpRoutedApp(
    tester,
    location: location,
    size: size,
    overrides: serviceOverrides(auth: MockAuthService(), users: users, user: user),
  );

  String locationOf(ProviderContainer container) =>
      container.read(appRouterProvider).routerDelegate.currentConfiguration.uri.path;

  Future<void> openMenu(WidgetTester tester, UserAccount user) async {
    await tester.tap(find.byTooltip('More actions').at([admin, bob].indexOf(user)));
    await tester.pumpAndSettle();
  }

  testWidgets('opens the user management from the top bar', (tester) async {
    final container = await pumpUsers(tester, location: '/files');

    await tester.tap(find.byTooltip('Administration'));
    await tester.pumpAndSettle();

    expect(locationOf(container), '/admin/users');
    expect(find.text('User management'), findsOneWidget);
    expect(find.text('bob@example.com'), findsOneWidget);
    expect(find.text('You'), findsOneWidget);
    expect(find.text('Back to files'), findsOneWidget);
  });

  testWidgets('redirects /admin to the users', (tester) async {
    final container = await pumpUsers(tester, location: '/admin');

    expect(locationOf(container), '/admin/users');
  });

  testWidgets('hides the administration from users', (tester) async {
    const user = CurrentUser(id: 'bob-id', username: 'bob', email: 'bob@example.com', isAdmin: false);
    final container = await pumpUsers(tester, user: user);

    expect(locationOf(container), '/403');
    container.read(appRouterProvider).go('/files');
    await tester.pumpAndSettle();
    expect(find.byTooltip('Administration'), findsNothing);
    expect(find.byTooltip('Settings'), findsOneWidget);
  });

  testWidgets('creates a user in a dialog', (tester) async {
    const carol = UserAccount(id: 'carol-id', username: 'carol', email: 'carol@example.com', isAdmin: true);
    when(() => users.create(any())).thenAnswer((_) async => carol);
    await pumpUsers(tester);

    await tester.tap(find.text('Create user'));
    await tester.pumpAndSettle();
    await tester.enterText(find.widgetWithText(TextFormField, 'Username'), 'carol');
    await tester.enterText(find.widgetWithText(TextFormField, 'Email'), 'carol@example.com');
    await tester.enterText(find.widgetWithText(TextFormField, 'Password'), 'long-password');
    await tester.tap(find.descendant(of: find.byType(SegmentedButton<bool>), matching: find.text('Administrator')));
    await tester.tap(find.widgetWithText(FilledButton, 'Create'));
    await tester.pumpAndSettle();

    verify(
      () => users.create(
        const UserRequest(username: 'carol', email: 'carol@example.com', password: 'long-password', isAdmin: true),
      ),
    ).called(1);
    expect(find.byType(AlertDialog), findsNothing);
    expect(find.text('User created'), findsOneWidget);
  });

  testWidgets('requires a password for a new user', (tester) async {
    await pumpUsers(tester);

    await tester.tap(find.text('Create user'));
    await tester.pumpAndSettle();
    await tester.enterText(find.widgetWithText(TextFormField, 'Username'), 'carol');
    await tester.enterText(find.widgetWithText(TextFormField, 'Email'), 'carol@example.com');
    await tester.tap(find.widgetWithText(FilledButton, 'Create'));
    await tester.pumpAndSettle();

    expect(find.text('Enter a value.'), findsOneWidget);
    verifyNever(() => users.create(any()));
  });

  testWidgets('edits a user from the name link and keeps the password', (tester) async {
    when(() => users.update('bob-id', any())).thenAnswer((_) async => bob.copyWith(username: 'robert'));
    await pumpUsers(tester);

    await tester.tap(find.text('bob'));
    await tester.pumpAndSettle();
    expect(find.text('Edit user'), findsOneWidget);
    expect(find.text('Leave empty to keep the current password.'), findsOneWidget);
    await tester.enterText(find.widgetWithText(TextFormField, 'Username'), 'robert');
    await tester.tap(find.widgetWithText(FilledButton, 'Save'));
    await tester.pumpAndSettle();

    verify(
      () => users.update(
        'bob-id',
        const UserRequest(username: 'robert', email: 'bob@example.com', password: null, isAdmin: false),
      ),
    ).called(1);
    expect(find.text('User saved'), findsOneWidget);
  });

  testWidgets('shows the reason when the server refuses a change', (tester) async {
    when(
      () => users.update('bob-id', any()),
    ).thenThrow(testProblem('/api/users/bob-id', status: 400, codes: ['DuplicateUserName']));
    await pumpUsers(tester);

    await openMenu(tester, bob);
    await tester.tap(find.text('Edit'));
    await tester.pumpAndSettle();
    await tester.enterText(find.widgetWithText(TextFormField, 'Username'), 'admin');
    await tester.tap(find.widgetWithText(FilledButton, 'Save'));
    await tester.pumpAndSettle();

    expect(find.text('This username is in use.'), findsOneWidget);
    expect(find.byType(AlertDialog), findsOneWidget);
  });

  testWidgets('deletes a user after a confirmation', (tester) async {
    when(() => users.delete('bob-id')).thenAnswer((_) async {});
    await pumpUsers(tester);

    await openMenu(tester, bob);
    await tester.tap(find.text('Delete user'));
    await tester.pumpAndSettle();
    expect(find.text('Delete bob?'), findsOneWidget);
    await tester.tap(find.widgetWithText(FilledButton, 'Delete user'));
    await tester.pumpAndSettle();

    verify(() => users.delete('bob-id')).called(1);
    expect(find.text('User deleted'), findsOneWidget);
  });

  testWidgets('does not let the admin delete the own user', (tester) async {
    await pumpUsers(tester);

    await openMenu(tester, admin);

    final item = tester.widget<PopupMenuItem<Object?>>(
      find.ancestor(
        of: find.text('Delete user'),
        matching: find.byWidgetPredicate((widget) => widget is PopupMenuItem),
      ),
    );
    expect(item.enabled, isFalse);
  });

  testWidgets('shows a list with menus in portrait', (tester) async {
    when(() => users.delete('bob-id')).thenAnswer((_) async {});
    await pumpUsers(tester, size: compact);

    expect(find.text('bob@example.com'), findsOneWidget);
    await openMenu(tester, bob);
    expect(find.text('Edit'), findsOneWidget);
    expect(find.text('Delete user'), findsOneWidget);
  });
}
