import 'package:flutter/gestures.dart';
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:guardou/api/generated/models/breadcrumb.dart';
import 'package:guardou/api/generated/models/item_kind.dart';
import 'package:guardou/app/router/app_router.dart';
import 'package:guardou/app/router/routes.dart';
import 'package:mocktail/mocktail.dart';

import '../../helpers/test_app.dart';

void main() {
  const portrait = Size(390, 844);
  const projectCrumb = Breadcrumb(id: 'project', name: 'project');

  final project = testEntry('project', parentId: null, kind: ItemKind.folder, canModify: false);
  final plan = testEntry('plan.txt', parentId: null, canModify: false);
  final notes = testEntry('notes.txt', parentId: 'project', canModify: false);

  late MockSharesService shares;
  late MockFilesService files;

  setUp(() {
    shares = MockSharesService();
    when(shares.sharedWithMe).thenAnswer((_) async => [plan, project]);
    files = stubFiles(
      folders: {
        Routes.rootFolderId: testFolder(testRoot, const []),
        'project': testFolder(project.copyWith(permissions: project.permissions.copyWith(canAddChildren: false)), [
          notes,
        ], shared: true),
        'docs': testFolder(
          testEntry('docs', parentId: 'project', kind: ItemKind.folder, canModify: false),
          const [],
          ancestors: [projectCrumb],
          shared: true,
        ),
      },
    );
  });

  Future<ProviderContainer> pumpShared(
    WidgetTester tester, {
    String location = Routes.shared,
    Size size = const Size(1280, 800),
  }) => pumpRoutedApp(
    tester,
    location: location,
    size: size,
    overrides: serviceOverrides(auth: MockAuthService(), files: files, shares: shares),
  );

  testWidgets('lists the items shared with me with folders first', (tester) async {
    await pumpShared(tester);

    expect(find.text('Shared with me'), findsWidgets);
    expect(tester.getTopLeft(find.text('project')).dy, lessThan(tester.getTopLeft(find.text('plan.txt')).dy));
  });

  testWidgets('opens a shared folder with the Shared with me breadcrumb', (tester) async {
    final container = await pumpShared(tester);

    await tester.tap(find.text('project'));
    await tester.pumpAndSettle();

    expect(container.read(appRouterProvider).state.uri.path, '/folders/project');
    expect(find.text('notes.txt'), findsOneWidget);
    expect(find.text('Shared with me'), findsWidgets);
    expect(find.byTooltip('You cannot add items to this folder.'), findsOneWidget);
  });

  testWidgets('goes back to Shared with me from the breadcrumb', (tester) async {
    final container = await pumpShared(tester, location: '/folders/docs');

    await tester.tap(find.descendant(of: find.byType(InkWell), matching: find.text('Shared with me')).last);
    await tester.pumpAndSettle();

    expect(container.read(appRouterProvider).state.uri.path, Routes.shared);
  });

  testWidgets('shows only open and download in the context menu', (tester) async {
    await pumpShared(tester);

    await tester.tap(find.text('plan.txt'), buttons: kSecondaryButton);
    await tester.pumpAndSettle();

    expect(find.text('Open'), findsOneWidget);
    expect(find.text('Download'), findsOneWidget);
    expect(find.text('Rename'), findsNothing);
    expect(find.text('Share'), findsNothing);
  });

  testWidgets('shows an empty state in portrait', (tester) async {
    when(shares.sharedWithMe).thenAnswer((_) async => []);

    await pumpShared(tester, size: portrait);

    expect(find.text('Nothing is shared with you'), findsOneWidget);
    expect(find.text('Items that other users share with you appear here.'), findsOneWidget);
  });
}
