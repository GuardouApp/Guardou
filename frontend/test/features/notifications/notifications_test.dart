import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:guardou/api/generated/models/item_kind.dart';
import 'package:guardou/api/generated/models/notification_entry.dart';
import 'package:guardou/api/generated/models/notification_item.dart';
import 'package:guardou/api/generated/models/notification_kind.dart';
import 'package:guardou/api/generated/models/user_summary.dart';
import 'package:guardou/core/browser/browser_notifications.dart';
import 'package:guardou/features/notifications/notifications_button.dart';
import 'package:guardou/features/notifications/notifications_section.dart';
import 'package:mocktail/mocktail.dart';

import '../../helpers/test_app.dart';

NotificationEntry shareNotification({String id = 'n1', bool read = false}) => NotificationEntry(
  id: id,
  kind: NotificationKind.shareReceived,
  created: DateTime.utc(2026, 11, 26, 12),
  read: read,
  item: const NotificationItem(id: 'docs-id', name: 'docs', kind: ItemKind.folder),
  sender: const UserSummary(
    id: 'bob-id',
    username: 'bob',
    displayName: 'Bob Jones',
    email: 'bob@example.com',
    hasPicture: false,
  ),
  version: null,
  url: null,
);

void main() {
  late MockAuthService auth;

  setUp(() => auth = MockAuthService());

  Future<void> pumpButton(WidgetTester tester, MockNotificationsService notifications) => pumpTestApp(
    tester,
    overrides: serviceOverrides(auth: auth, notifications: notifications),
    child: const Scaffold(body: Align(alignment: Alignment.topRight, child: NotificationsButton())),
  );

  testWidgets('shows the unread count and marks the notifications read after the panel closes', (tester) async {
    final notifications = stubNotifications(notifications: [shareNotification()]);
    await pumpButton(tester, notifications);

    await tester.tap(find.byTooltip('Notifications (1 unread)'));
    await tester.pumpAndSettle();
    expect(find.text('Bob Jones shared “docs” with you'), findsOneWidget);

    await tester.tapAt(const Offset(10, 790));
    await tester.pumpAndSettle();

    verify(notifications.markAllRead).called(1);
    expect(find.byTooltip('Notifications'), findsOneWidget);
  });

  testWidgets('clears the history', (tester) async {
    final notifications = stubNotifications(notifications: [shareNotification(read: true)]);
    await pumpButton(tester, notifications);

    await tester.tap(find.byTooltip('Notifications'));
    await tester.pumpAndSettle();
    await tester.tap(find.text('Clear all'));
    await tester.pumpAndSettle();

    verify(notifications.clear).called(1);
    expect(find.text('You have no notifications.'), findsOneWidget);
    verifyNever(notifications.markAllRead);
  });

  for (final size in [const Size(1280, 800), const Size(390, 844)]) {
    testWidgets('keeps the panel in the window at $size', (tester) async {
      await pumpTestApp(
        tester,
        size: size,
        overrides: serviceOverrides(auth: auth, notifications: stubNotifications(notifications: [shareNotification()])),
        child: const Scaffold(body: Align(alignment: Alignment.topRight, child: NotificationsButton())),
      );

      await tester.tap(find.byType(NotificationsButton));
      await tester.pumpAndSettle();

      final panel = tester.getRect(find.byType(Material).last);
      expect(panel.left, greaterThanOrEqualTo(8));
      expect(panel.right, lessThanOrEqualTo(size.width - 8));
    });
  }

  group('browser notifications', () {
    Future<void> pumpSection(WidgetTester tester, FakeBrowserNotifications browser) async {
      await pumpTestApp(
        tester,
        overrides: serviceOverrides(auth: auth, browserNotifications: browser),
        child: const Scaffold(body: SingleChildScrollView(child: NotificationsSection())),
      );
      await tester.tap(find.text('Notifications'));
      await tester.pumpAndSettle();
    }

    testWidgets('turns on after the browser grants the permission', (tester) async {
      final browser = FakeBrowserNotifications(answer: BrowserNotificationPermission.granted);
      await pumpSection(tester, browser);

      await tester.tap(find.byType(Switch));
      await tester.pumpAndSettle();

      expect(tester.widget<Switch>(find.byType(Switch)).value, isTrue);
    });

    testWidgets('stays off when the browser denies the permission', (tester) async {
      final browser = FakeBrowserNotifications(answer: BrowserNotificationPermission.denied);
      await pumpSection(tester, browser);

      await tester.tap(find.byType(Switch));
      await tester.pumpAndSettle();

      expect(tester.widget<Switch>(find.byType(Switch)).value, isFalse);
      expect(find.textContaining('The browser blocks notifications'), findsWidgets);
    });

    testWidgets('is not available without browser support', (tester) async {
      await pumpSection(tester, FakeBrowserNotifications(permission: BrowserNotificationPermission.unsupported));

      expect(tester.widget<Switch>(find.byType(Switch)).onChanged, isNull);
      expect(find.text('This browser does not support notifications.'), findsOneWidget);
    });
  });
}
