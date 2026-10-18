import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:guardou/api/generated/models/task_entry.dart';
import 'package:guardou/api/generated/models/task_kind.dart';
import 'package:guardou/api/generated/models/task_priority.dart';
import 'package:guardou/api/generated/models/task_queue_summary.dart';
import 'package:guardou/api/generated/models/task_state.dart';
import 'package:guardou/app/router/app_router.dart';
import 'package:mocktail/mocktail.dart';

import '../../helpers/test_app.dart';

void main() {
  const compact = Size(390, 844);
  const previews = TaskQueueSummary(kind: TaskKind.preview, pending: 4, running: 2, failed: 1);

  TaskEntry task(String id, String name, TaskState state, {String? error}) => TaskEntry(
    id: id,
    kind: TaskKind.preview,
    state: state,
    priority: TaskPriority.normal,
    itemId: 'item-$id',
    itemName: name,
    owner: 'bob',
    error: error,
    created: DateTime.utc(2026, 1, 1),
    updated: DateTime.utc(2026, 1, 1),
  );

  final pending = task('pending-id', 'beach.jpg', TaskState.pending);
  final failed = task('failed-id', 'broken.png', TaskState.failed, error: 'Not an image');

  late MockTasksService tasks;

  setUp(() {
    tasks = MockTasksService();
    when(tasks.queues).thenAnswer((_) async => [previews]);
    when(() => tasks.list(TaskKind.preview, TaskState.pending)).thenAnswer((_) async => [pending]);
    when(() => tasks.list(TaskKind.preview, TaskState.failed)).thenAnswer((_) async => [failed]);
    when(() => tasks.list(TaskKind.preview, TaskState.running)).thenAnswer((_) async => []);
  });

  Future<ProviderContainer> pumpTasks(WidgetTester tester, {Size size = const Size(1280, 800)}) => pumpRoutedApp(
    tester,
    location: '/admin/tasks',
    size: size,
    overrides: serviceOverrides(auth: MockAuthService(), tasks: tasks),
  );

  Future<void> openCounter(WidgetTester tester, String label) async {
    await tester.tap(find.text(label));
    await tester.pumpAndSettle();
  }

  Future<void> chooseTaskAction(WidgetTester tester, String action) async {
    await tester.tap(find.byTooltip('More actions'));
    await tester.pumpAndSettle();
    await tester.tap(find.text(action));
    await tester.pumpAndSettle();
  }

  testWidgets('opens the task queue from the admin navigation', (tester) async {
    final users = MockUsersService();
    when(users.list).thenAnswer((_) async => []);
    final container = await pumpRoutedApp(
      tester,
      location: '/admin/users',
      overrides: serviceOverrides(auth: MockAuthService(), users: users, tasks: tasks),
    );

    await tester.tap(find.text('Tasks'));
    await tester.pumpAndSettle();

    expect(container.read(appRouterProvider).state.uri.path, '/admin/tasks');
    expect(find.text('Task queue'), findsOneWidget);
  });

  testWidgets('shows a card for each queue with the counters', (tester) async {
    await pumpTasks(tester);

    expect(find.text('Generate previews'), findsOneWidget);
    for (final text in ['Running', 'Queued', 'Failed', '2', '4', '1', 'All']) {
      expect(find.text(text), findsOneWidget);
    }
  });

  testWidgets('lists the tasks of a counter and runs one immediately', (tester) async {
    when(() => tasks.run('pending-id')).thenAnswer((_) async => null);
    await pumpTasks(tester);

    await openCounter(tester, 'Queued');
    expect(find.text('Generate previews: Queued'), findsOneWidget);
    expect(find.text('beach.jpg'), findsOneWidget);
    await chooseTaskAction(tester, 'Run now');

    verify(() => tasks.run('pending-id')).called(1);
    expect(find.text('The task is complete.'), findsOneWidget);
  });

  testWidgets('resets a failed task and retries all failed tasks', (tester) async {
    when(() => tasks.reset('failed-id')).thenAnswer((_) async => failed.copyWith(state: TaskState.pending));
    when(() => tasks.resetFailed(TaskKind.preview)).thenAnswer((_) async {});
    await pumpTasks(tester);

    await openCounter(tester, 'Failed');
    expect(find.textContaining('Not an image'), findsOneWidget);
    await chooseTaskAction(tester, 'Reset');
    await tester.tap(find.text('Retry failed tasks'));
    await tester.pumpAndSettle();

    verify(() => tasks.reset('failed-id')).called(1);
    verify(() => tasks.resetFailed(TaskKind.preview)).called(1);
    expect(find.text('The failed tasks are pending again.'), findsOneWidget);
  });

  testWidgets('shows a message when a task is busy', (tester) async {
    when(() => tasks.run('pending-id')).thenThrow(testProblem('/api/tasks/pending-id/run', status: 409));
    await pumpTasks(tester);

    await openCounter(tester, 'Queued');
    await chooseTaskAction(tester, 'Run now');

    expect(find.text('The task is running. Try again later.'), findsOneWidget);
  });

  testWidgets('processes all files after a confirmation', (tester) async {
    when(() => tasks.queueAll(TaskKind.preview)).thenAnswer((_) async {});
    await pumpTasks(tester);

    await tester.tap(find.text('All'));
    await tester.pumpAndSettle();
    expect(find.text('Process all files again?'), findsOneWidget);
    await tester.tap(find.widgetWithText(FilledButton, 'All'));
    await tester.pumpAndSettle();

    verify(() => tasks.queueAll(TaskKind.preview)).called(1);
    expect(find.text('All files are in the queue.'), findsOneWidget);
  });

  testWidgets('portrait puts the queue actions under the counters', (tester) async {
    await pumpTasks(tester, size: compact);

    expect(tester.getTopLeft(find.text('All')).dy, greaterThan(tester.getBottomLeft(find.text('Queued')).dy));
    await openCounter(tester, 'Failed');
    expect(find.text('broken.png'), findsOneWidget);
  });

  testWidgets('shows an empty list', (tester) async {
    await pumpTasks(tester);

    await openCounter(tester, 'Running');

    expect(find.text('No tasks'), findsOneWidget);
  });
}
