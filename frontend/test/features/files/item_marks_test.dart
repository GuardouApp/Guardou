import 'package:fluentui_system_icons/fluentui_system_icons.dart';
import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:guardou/api/generated/models/item_kind.dart';
import 'package:guardou/app/router/routes.dart';
import 'package:guardou/features/files/item_marks.dart';

import '../../helpers/test_app.dart';

void main() {
  final shared = testEntry('shared.txt', sharedByMe: true);
  final both = testEntry('both.txt', sharedByMe: true, starred: true);
  final private = testEntry('private.txt');
  final sharedFolder = testEntry('shared folder', kind: ItemKind.folder, sharedByMe: true, starred: true);

  late MockFilesService files;

  setUp(() {
    files = stubFiles(
      folders: {
        Routes.rootFolderId: testFolder(testRoot, [both, private, shared, sharedFolder]),
      },
    );
  });

  Future<void> pumpFiles(WidgetTester tester, {Size size = const Size(1280, 800)}) => pumpRoutedApp(
    tester,
    location: Routes.files,
    size: size,
    overrides: serviceOverrides(auth: MockAuthService(), files: files),
  );

  testWidgets('shows the shared badge on the items shared by me', (tester) async {
    await pumpFiles(tester);

    expect(find.descendant(of: find.byType(ItemMarks), matching: find.byType(SharedBadge)), findsNWidgets(3));
    expect(
      find.byWidgetPredicate((widget) => widget is Icon && widget.semanticLabel == 'Shared by you'),
      findsNWidgets(3),
    );
  });

  testWidgets('shows the shared badge at the bottom left of the preview in the grid view', (tester) async {
    await pumpFiles(tester, size: const Size(390, 844));

    await tester.tap(find.byTooltip('Show as grid'));
    await tester.pumpAndSettle();

    final preview = find.ancestor(of: find.byType(SharedBadge), matching: find.byType(PreviewMarks)).first;
    final shared = find.descendant(of: preview, matching: find.byType(SharedBadge));
    final star = find.descendant(of: preview, matching: find.byType(StarBadge));
    expect(find.byType(PreviewMarks), findsNWidgets(3));
    expect(tester.getBottomLeft(shared).dx, closeTo(tester.getBottomLeft(preview).dx, 8));
    expect(tester.getBottomRight(star).dx, closeTo(tester.getBottomRight(preview).dx, 8));
    expect(find.descendant(of: find.byType(ItemMarks), matching: find.byType(SharedBadge)), findsNothing);
  });

  testWidgets('shows the folder marks before the more button in the grid view', (tester) async {
    await pumpFiles(tester);

    await tester.tap(find.byTooltip('Show as grid'));
    await tester.pumpAndSettle();

    final indicators = find.byType(ItemIndicators);
    final shared = find.descendant(of: indicators, matching: find.byIcon(FluentIcons.people_24_filled));
    final star = find.descendant(of: indicators, matching: find.byIcon(FluentIcons.star_24_filled));
    final more = find.byTooltip('More actions').first;
    expect(shared, findsOneWidget);
    expect(star, findsOneWidget);
    expect(tester.getCenter(star).dy, closeTo(tester.getCenter(more).dy, 1));
    expect(tester.getTopRight(shared).dx, lessThanOrEqualTo(tester.getTopLeft(star).dx));
    expect(tester.getTopRight(star).dx, lessThanOrEqualTo(tester.getTopLeft(more).dx));
    expect(find.byType(ItemMarks), findsNothing);
  });
}
