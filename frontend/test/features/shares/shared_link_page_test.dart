import 'package:dio/dio.dart';
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:guardou/api/generated/models/breadcrumb.dart';
import 'package:guardou/api/generated/models/item_creator.dart';
import 'package:guardou/api/generated/models/item_kind.dart';
import 'package:guardou/app/router/app_router.dart';
import 'package:guardou/features/shares/shared_file_source.dart';
import 'package:mocktail/mocktail.dart';

import '../../helpers/test_app.dart';

void main() {
  const token = 'AAAAAAAAAAAAAAAAAAAAAA';
  const portrait = Size(390, 844);
  final clip = testEntry('clip.mp4', id: 'clip-id', parentId: null, canModify: false, mimeType: 'video/mp4');
  final docs = testEntry('docs', id: 'docs-id', parentId: null, kind: ItemKind.folder, canModify: false);
  final inner = testEntry('inner', id: 'inner-id', parentId: docs.id, kind: ItemKind.folder, canModify: false);
  final first = testEntry('a-photo.mp4', id: 'first-id', parentId: inner.id, canModify: false, mimeType: 'video/mp4');
  final second = testEntry(
    'b-song.mp3',
    id: 'second-id',
    parentId: inner.id,
    canModify: false,
    mimeType: 'audio/mpeg',
  ).copyWith(creator: const ItemCreator(id: 'zoe-id', displayName: 'Zoe Owner', hasPicture: false));
  const docsCrumb = Breadcrumb(id: 'docs-id', name: 'docs');
  const innerCrumb = Breadcrumb(id: 'inner-id', name: 'inner');

  late MockSharesService shares;
  late MockBrowserFiles browser;

  setUp(() {
    shares = MockSharesService();
    browser = stubBrowser();
    when(() => shares.files(token)).thenReturn(SharedFileSource(Dio(), Uri.parse('http://localhost'), token));
    final known = {
      clip.id: testFolder(clip, const []),
      docs.id: testFolder(docs, [inner]),
      inner.id: testFolder(inner, [first, second], ancestors: const [docsCrumb]),
      first.id: testFolder(first, const [], ancestors: const [docsCrumb, innerCrumb]),
    };
    when(() => shares.sharedDetails(token, any())).thenAnswer((invocation) async {
      final id = invocation.positionalArguments.last as String;
      return known[id] ?? (throw testNotFound(id));
    });
  });

  Future<ProviderContainer> pumpShare(WidgetTester tester, String location, {Size size = const Size(1280, 800)}) =>
      pumpRoutedApp(
        tester,
        location: location,
        size: size,
        overrides: serviceOverrides(auth: MockAuthService(), user: null, shares: shares, browser: browser),
      );

  testWidgets('shows a shared file without a session', (tester) async {
    when(() => shares.sharedItem(token)).thenAnswer((_) async => clip);

    await pumpShare(tester, '/s/$token');

    expect(find.text(clip.name), findsOneWidget);
    expect(find.text('video http://localhost/api/shares/$token/items/${clip.id}/view'), findsOneWidget);
    expect(find.byTooltip('Close'), findsNothing);
  });

  testWidgets('lists a shared folder and opens a subfolder in portrait', (tester) async {
    when(() => shares.sharedItem(token)).thenAnswer((_) async => docs);

    await pumpShare(tester, '/s/$token', size: portrait);
    await tester.tap(find.text('inner'));
    await tester.pumpAndSettle();

    expect(find.text(first.name), findsOneWidget);
    expect(find.text(second.name), findsOneWidget);
    expect(find.text('docs'), findsWidgets);
  });

  testWidgets('shows the creator of each file, not the viewer', (tester) async {
    await pumpShare(tester, '/s/$token/folders/${inner.id}');
    await tester.tap(find.byTooltip('Show as grid'));
    await tester.pumpAndSettle();

    expect(find.text('Z'), findsOneWidget);
    expect(find.text('A'), findsOneWidget);
    expect(find.textContaining('Zoe Owner created'), findsOneWidget);
    expect(find.textContaining('You created'), findsNothing);
  });

  testWidgets('steps through the media files of a shared folder', (tester) async {
    final container = await pumpShare(tester, '/s/$token/view/${first.id}');

    await tester.tap(find.byTooltip('Next file'));
    await tester.pumpAndSettle();

    expect(container.read(appRouterProvider).routeInformationProvider.value.uri.path, '/s/$token/view/${second.id}');
  });

  testWidgets('closes the viewer to the shared folder', (tester) async {
    final container = await pumpShare(tester, '/s/$token/view/${first.id}');

    await tester.tap(find.byTooltip('Close'));
    await tester.pumpAndSettle();

    expect(container.read(appRouterProvider).routeInformationProvider.value.uri.path, '/s/$token/folders/${inner.id}');
  });

  testWidgets('downloads the shared folder', (tester) async {
    when(() => shares.sharedItem(token)).thenAnswer((_) async => docs);

    await pumpShare(tester, '/s/$token');
    await tester.tap(find.byTooltip('Download'));

    verify(() => browser.download(Uri.parse('http://localhost/api/shares/$token/items/${docs.id}/download'))).called(1);
  });

  testWidgets('tells that an unknown link is not available', (tester) async {
    when(() => shares.sharedItem(token)).thenThrow(testNotFound('/api/shares/$token'));

    await pumpShare(tester, '/s/$token');

    expect(find.text('This link does not exist, or the owner stopped sharing it.'), findsOneWidget);
  });
}
