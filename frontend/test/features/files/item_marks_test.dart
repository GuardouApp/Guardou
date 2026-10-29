import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:guardou/app/router/routes.dart';
import 'package:guardou/features/files/item_marks.dart';

import '../../helpers/test_app.dart';

void main() {
  final shared = testEntry('shared.txt', sharedByMe: true);
  final both = testEntry('both.txt', sharedByMe: true, starred: true);
  final private = testEntry('private.txt');

  late MockFilesService files;

  setUp(() {
    files = stubFiles(
      folders: {
        Routes.rootFolderId: testFolder(testRoot, [both, private, shared]),
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

    expect(find.descendant(of: find.byType(ItemMarks), matching: find.byType(SharedBadge)), findsNWidgets(2));
    expect(
      find.byWidgetPredicate((widget) => widget is Icon && widget.semanticLabel == 'Shared by you'),
      findsNWidgets(2),
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
}
