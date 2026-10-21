import 'package:flutter/gestures.dart';
import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:guardou/api/generated/models/trash_entry.dart';
import 'package:mocktail/mocktail.dart';

import '../../helpers/test_app.dart';

void main() {
  late MockTrashService trash;

  TrashEntry trashed(String name) =>
      TrashEntry(entry: testEntry(name), trashed: DateTime.utc(2026, 1, 1), expires: DateTime.utc(2026, 1, 31));

  setUp(() {
    trash = MockTrashService();
    when(trash.list).thenAnswer((_) async => [trashed('old.txt'), trashed('older.txt')]);
  });

  Future<void> pumpTrash(WidgetTester tester, {Size size = const Size(1280, 800)}) => pumpRoutedApp(
    tester,
    location: '/trash',
    size: size,
    overrides: serviceOverrides(auth: MockAuthService(), trash: trash),
  );

  testWidgets('lists the items in the trash', (tester) async {
    await pumpTrash(tester);

    expect(find.text('old.txt'), findsOneWidget);
    expect(find.text('older.txt'), findsOneWidget);
    expect(find.text('Items in the trash are deleted forever after 30 days.'), findsOneWidget);
  });

  testWidgets('restores the selected items', (tester) async {
    when(() => trash.restore(['old.txt'])).thenAnswer((_) async => [testEntry('old.txt')]);
    await pumpTrash(tester);

    await tester.tap(find.text('old.txt'));
    await tester.pumpAndSettle();
    await tester.tap(find.byTooltip('Restore'));
    await tester.pumpAndSettle();

    verify(() => trash.restore(['old.txt'])).called(1);
    expect(find.text('1 item restored'), findsOneWidget);
  });

  testWidgets('deletes items forever after a confirmation', (tester) async {
    when(() => trash.delete(['older.txt'])).thenAnswer((_) async {});
    await pumpTrash(tester);

    await tester.tap(find.text('older.txt'), buttons: kSecondaryButton);
    await tester.pumpAndSettle();
    await tester.tap(find.text('Delete forever'));
    await tester.pumpAndSettle();
    expect(find.text('Delete 1 item forever?'), findsOneWidget);
    await tester.tap(find.widgetWithText(FilledButton, 'Delete forever'));
    await tester.pumpAndSettle();

    verify(() => trash.delete(['older.txt'])).called(1);
  });

  testWidgets('empties the trash after a confirmation', (tester) async {
    when(trash.empty).thenAnswer((_) async {});
    await pumpTrash(tester);

    await tester.tap(find.text('Empty trash'));
    await tester.pumpAndSettle();
    await tester.tap(find.widgetWithText(FilledButton, 'Empty trash'));
    await tester.pumpAndSettle();

    verify(trash.empty).called(1);
    expect(find.text('The trash is empty'), findsWidgets);
  });

  testWidgets('shows an empty trash in portrait', (tester) async {
    when(trash.list).thenAnswer((_) async => []);
    await pumpTrash(tester, size: const Size(390, 844));

    expect(find.text('Items that you move to the trash show here.'), findsOneWidget);
  });
}
