import 'package:flutter/gestures.dart';
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:guardou/api/generated/models/item_kind.dart';
import 'package:guardou/app/router/routes.dart';
import 'package:guardou/features/files/star_mark.dart';
import 'package:lucide_icons_flutter/lucide_icons.dart';
import 'package:mocktail/mocktail.dart';

import '../../helpers/test_app.dart';

void main() {
  const portrait = Size(390, 844);

  final docs = testEntry('docs', kind: ItemKind.folder, starred: true);
  final plan = testEntry('plan.txt', starred: true);
  final notes = testEntry('notes.txt');

  late MockFilesService files;

  setUp(() {
    files = stubFiles(
      folders: {
        Routes.rootFolderId: testFolder(testRoot, [notes]),
      },
    );
    when(files.starred).thenAnswer((_) async => [plan, docs]);
    when(() => files.star(any())).thenAnswer((_) async {});
    when(() => files.unstar(any())).thenAnswer((_) async {});
  });

  Future<ProviderContainer> pumpStarred(
    WidgetTester tester, {
    String location = Routes.starred,
    Size size = const Size(1280, 800),
  }) => pumpRoutedApp(
    tester,
    location: location,
    size: size,
    overrides: serviceOverrides(auth: MockAuthService(), files: files),
  );

  testWidgets('lists the starred items with folders first', (tester) async {
    await pumpStarred(tester);

    expect(find.text('Starred'), findsWidgets);
    expect(tester.getTopLeft(find.text('docs')).dy, lessThan(tester.getTopLeft(find.text('plan.txt')).dy));
    expect(find.descendant(of: find.byType(StarMark), matching: find.byIcon(LucideIcons.star)), findsNWidgets(2));
  });

  testWidgets('shows the star on the file preview in the grid view', (tester) async {
    await pumpStarred(tester);

    await tester.tap(find.byTooltip('Show as grid'));
    await tester.pumpAndSettle();

    final preview = find.ancestor(of: find.byType(StarBadge), matching: find.byType(AspectRatio));
    final badge = find.descendant(of: preview, matching: find.byType(StarBadge));
    expect(preview, findsOneWidget);
    expect(tester.getBottomRight(badge).dx, closeTo(tester.getBottomRight(preview).dx, 8));
    expect(find.descendant(of: find.byType(StarMark), matching: find.byType(StarBadge)), findsOneWidget);
  });

  testWidgets('removes a star from the context menu', (tester) async {
    await pumpStarred(tester);

    await tester.tap(find.text('plan.txt'), buttons: kSecondaryButton);
    await tester.pumpAndSettle();

    expect(find.text('Add to Starred'), findsNothing);
    await tester.tap(find.text('Remove from Starred'));
    await tester.pumpAndSettle();

    verify(() => files.unstar(['plan.txt'])).called(1);
    expect(find.text('1 item removed from Starred'), findsOneWidget);
  });

  testWidgets('adds a star from the selection toolbar', (tester) async {
    await pumpStarred(tester, location: Routes.files);
    await tester.tap(find.text('notes.txt'));
    await tester.pumpAndSettle();
    await tester.tap(find.byTooltip('Add to Starred'));
    await tester.pumpAndSettle();

    verify(() => files.star(['notes.txt'])).called(1);
    expect(find.byTooltip('Remove from Starred'), findsNothing);
  });

  testWidgets('shows an empty state in portrait', (tester) async {
    when(files.starred).thenAnswer((_) async => []);

    await pumpStarred(tester, size: portrait);

    expect(find.text('No starred items'), findsOneWidget);
    expect(find.text('Add a star to folders and files that you use frequently.'), findsOneWidget);
  });
}
