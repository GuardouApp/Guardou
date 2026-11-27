import 'package:flutter_test/flutter_test.dart';
import 'package:guardou/app/router/routes.dart';
import 'package:guardou/core/realtime/realtime_connection.dart';
import 'package:guardou/core/realtime/realtime_event.dart';
import 'package:guardou/features/shares/shares_service.dart';
import 'package:mocktail/mocktail.dart';

import '../../helpers/test_app.dart';
import '../notifications/notifications_test.dart';

void main() {
  late MockAuthService auth;
  late FakeRealtimeConnection realtime;

  setUp(() {
    auth = MockAuthService();
    realtime = FakeRealtimeConnection();
  });

  Future<void> pumpFiles(WidgetTester tester, {MockFilesService? files, SharesService? shares}) => pumpRoutedApp(
    tester,
    location: Routes.files,
    overrides: serviceOverrides(auth: auth, files: files, shares: shares, realtime: realtime),
  );

  testWidgets('connects while a user is signed in', (tester) async {
    await pumpFiles(tester);

    expect(realtime.started, isTrue);
  });

  testWidgets('does not connect without a session', (tester) async {
    await pumpRoutedApp(
      tester,
      location: Routes.login,
      overrides: serviceOverrides(auth: auth, user: null, realtime: realtime),
    );

    expect(realtime.started, isFalse);
  });

  testWidgets('loads the folder again when it changes', (tester) async {
    final files = stubFiles();
    await pumpFiles(tester, files: files);
    clearInteractions(files);

    realtime.emit(const ItemsChanged({testRootId}));
    await tester.pumpAndSettle();

    verify(() => files.item(Routes.rootFolderId)).called(1);
  });

  testWidgets('ignores changes of other folders', (tester) async {
    final files = stubFiles();
    await pumpFiles(tester, files: files);
    clearInteractions(files);

    realtime.emit(const ItemsChanged({'other-folder'}));
    await tester.pumpAndSettle();

    verifyNever(() => files.item(Routes.rootFolderId));
  });

  testWidgets('shows a received notification', (tester) async {
    await pumpFiles(tester);

    realtime.emit(NotificationReceived(shareNotification()));
    await tester.pumpAndSettle();

    expect(find.text('Bob Jones shared “docs” with you'), findsOneWidget);
    expect(find.byTooltip('Notifications (1 unread)'), findsOneWidget);
  });

  testWidgets('shows the connection state and loads the data again after a reconnect', (tester) async {
    final files = stubFiles();
    await pumpFiles(tester, files: files);
    realtime.status(RealtimeStatus.connected);
    await tester.pumpAndSettle();

    realtime.status(RealtimeStatus.reconnecting);
    await tester.pump();
    await tester.pump(const Duration(milliseconds: 500));
    expect(find.text('The connection to the server is lost. Connecting again…'), findsOneWidget);
    clearInteractions(files);

    realtime.status(RealtimeStatus.connected);
    await tester.pumpAndSettle();

    expect(find.text('The connection to the server is available again.'), findsOneWidget);
    verify(() => files.item(Routes.rootFolderId)).called(1);
  });
}
