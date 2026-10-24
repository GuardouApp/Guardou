import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:guardou/core/widgets/labeled_text_field.dart';
import 'package:guardou/api/generated/models/item_kind.dart';
import 'package:guardou/api/generated/models/user_summary.dart';
import 'package:guardou/features/files/item_action.dart';
import 'package:guardou/features/shares/share_dialog.dart';
import 'package:mocktail/mocktail.dart';

import '../../helpers/test_app.dart';

void main() {
  const link = 'http://localhost/s/AAAAAAAAAAAAAAAAAAAAAA';
  const portrait = Size(390, 844);
  final file = testEntry('report.txt', id: 'file-id');
  const bob = UserSummary(
    id: 'bob-id',
    username: 'bob',
    displayName: 'Bob Jones',
    email: 'bob@example.com',
    hasPicture: false,
  );

  late MockSharesService shares;

  setUp(() {
    shares = MockSharesService();
    when(() => shares.share(file.id)).thenAnswer((_) async => link);
    when(() => shares.stopSharing(file.id)).thenAnswer((_) async {});
    when(() => shares.recipients(file.id)).thenAnswer((_) async => []);
  });

  Future<void> pumpDialog(WidgetTester tester, {Size size = const Size(1280, 800)}) => pumpTestApp(
    tester,
    size: size,
    overrides: serviceOverrides(auth: MockAuthService(), shares: shares),
    child: ShareDialog(entry: file),
  );

  Future<void> choose(WidgetTester tester, String current, String option) async {
    await tester.tap(find.text(current));
    await tester.pumpAndSettle();
    await tester.tap(find.text(option).last);
    await tester.pumpAndSettle();
  }

  testWidgets('shows restricted access for a file that is not shared', (tester) async {
    when(() => shares.link(file.id)).thenAnswer((_) async => null);

    await pumpDialog(tester);

    expect(find.text('Share "report.txt"'), findsOneWidget);
    expect(find.text('Restricted'), findsOneWidget);
    expect(find.text('Only people with access can open this item.'), findsOneWidget);
    expect(find.text('No other user has access.'), findsOneWidget);
    expect(find.text('Copy link'), findsNothing);
  });

  testWidgets('shares the file with a user from the search', (tester) async {
    when(() => shares.searchUsers('bo')).thenAnswer((_) async => [bob]);
    when(() => shares.addRecipient(file.id, bob.id)).thenAnswer((_) async => [bob]);

    await pumpDialog(tester);
    await tester.enterText(find.widgetWithText(LabeledTextField, 'Add people'), 'bo');
    await tester.pumpAndSettle();
    await tester.tap(find.text('bob@example.com'));
    await tester.pumpAndSettle();

    verify(() => shares.addRecipient(file.id, bob.id)).called(1);
    expect(find.text('Bob Jones'), findsOneWidget);
    expect(find.text('No other user has access.'), findsNothing);
  });

  testWidgets('removes the access of a user in portrait', (tester) async {
    when(() => shares.link(file.id)).thenAnswer((_) async => null);
    when(() => shares.recipients(file.id)).thenAnswer((_) async => [bob]);
    when(() => shares.removeRecipient(file.id, bob.id)).thenAnswer((_) async {});

    await pumpDialog(tester, size: portrait);
    await tester.tap(find.byTooltip('Remove access'));
    await tester.pumpAndSettle();

    verify(() => shares.removeRecipient(file.id, bob.id)).called(1);
    expect(find.text('Bob Jones'), findsNothing);
  });

  testWidgets('does not suggest a user who has access', (tester) async {
    when(() => shares.link(file.id)).thenAnswer((_) async => null);
    when(() => shares.recipients(file.id)).thenAnswer((_) async => [bob]);
    when(() => shares.searchUsers('bo')).thenAnswer((_) async => [bob]);

    await pumpDialog(tester);
    await tester.enterText(find.widgetWithText(LabeledTextField, 'Add people'), 'bo');
    await tester.pumpAndSettle();

    expect(find.text('bob@example.com'), findsOneWidget);
  });

  testWidgets('shares the file with anyone who has the link in portrait', (tester) async {
    when(() => shares.link(file.id)).thenAnswer((_) async => null);

    await pumpDialog(tester, size: portrait);
    await choose(tester, 'Restricted', 'Anyone with the link');

    verify(() => shares.share(file.id)).called(1);
    expect(find.text(link), findsOneWidget);
    expect(find.text('Copy link'), findsOneWidget);
  });

  testWidgets('restricts a shared file', (tester) async {
    when(() => shares.link(file.id)).thenAnswer((_) async => link);

    await pumpDialog(tester);
    await choose(tester, 'Anyone with the link', 'Restricted');

    verify(() => shares.stopSharing(file.id)).called(1);
    expect(find.text(link), findsNothing);
  });

  test('enables sharing only for one item that the user can modify', () {
    final folder = testEntry('docs', kind: ItemKind.folder);
    final readOnly = testEntry('a.txt', canModify: false);

    expect(ItemAction.share.isEnabledFor([file]), isTrue);
    expect(ItemAction.share.isEnabledFor([folder]), isTrue);
    expect(ItemAction.share.isEnabledFor([testRoot]), isFalse);
    expect(ItemAction.share.isEnabledFor([readOnly]), isFalse);
    expect(ItemAction.share.isEnabledFor([file, testEntry('b.txt')]), isFalse);
  });
}
