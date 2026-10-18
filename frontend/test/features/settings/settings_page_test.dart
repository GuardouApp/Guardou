import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:guardou/api/generated/models/task_concurrency.dart';
import 'package:guardou/api/generated/models/task_kind.dart';
import 'package:guardou/app/router/app_router.dart';
import 'package:mocktail/mocktail.dart';

import '../../helpers/test_app.dart';

void main() {
  const previews = TaskConcurrency(kind: TaskKind.preview, concurrency: 2);
  const compact = Size(390, 844);

  late MockSettingsService settings;

  setUp(() {
    settings = MockSettingsService();
    when(settings.taskSettings).thenAnswer((_) async => [previews]);
  });

  Future<ProviderContainer> pumpSettings(WidgetTester tester, {Size size = const Size(1280, 800)}) => pumpRoutedApp(
    tester,
    location: '/admin/settings',
    size: size,
    overrides: serviceOverrides(auth: MockAuthService(), settings: settings),
  );

  Finder concurrencyField() => find.widgetWithText(TextFormField, 'Generate previews: tasks at the same time');

  testWidgets('opens the settings from the admin navigation', (tester) async {
    final users = MockUsersService();
    when(users.list).thenAnswer((_) async => []);
    final container = await pumpRoutedApp(
      tester,
      location: '/admin/users',
      overrides: serviceOverrides(auth: MockAuthService(), users: users, settings: settings),
    );

    await tester.tap(find.text('Settings').last);
    await tester.pumpAndSettle();

    expect(container.read(appRouterProvider).state.uri.path, '/admin/settings');
    expect(find.text('System settings'), findsOneWidget);
    expect(find.text('Task settings'), findsOneWidget);
  });

  testWidgets('saves the concurrency of a queue', (tester) async {
    when(
      () => settings.changeTaskSettings(any()),
    ).thenAnswer((invocation) async => invocation.positionalArguments.single as List<TaskConcurrency>);
    await pumpSettings(tester);

    await tester.enterText(concurrencyField(), '6');
    await tester.tap(find.widgetWithText(FilledButton, 'Save'));
    await tester.pumpAndSettle();

    verify(() => settings.changeTaskSettings([previews.copyWith(concurrency: 6)])).called(1);
    expect(find.text('The settings are saved.'), findsOneWidget);
  });

  testWidgets('refuses a concurrency out of range', (tester) async {
    await pumpSettings(tester);

    await tester.enterText(concurrencyField(), '17');
    await tester.tap(find.widgetWithText(FilledButton, 'Save'));
    await tester.pumpAndSettle();

    expect(find.text('Enter a number from 1 to 16.'), findsOneWidget);
    verifyNever(() => settings.changeTaskSettings(any()));
  });

  testWidgets('works in portrait', (tester) async {
    await pumpSettings(tester, size: compact);

    expect(concurrencyField(), findsOneWidget);
    expect(find.widgetWithText(FilledButton, 'Save'), findsOneWidget);
  });
}
