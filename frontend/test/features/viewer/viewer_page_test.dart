import 'dart:convert';

import 'package:dio/dio.dart';
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:guardou/api/generated/models/preview_size.dart';
import 'package:guardou/app/router/app_router.dart';
import 'package:guardou/features/viewer/viewer_page.dart';
import 'package:mocktail/mocktail.dart';

import '../../helpers/test_app.dart';

void main() {
  const portrait = Size(390, 844);

  final first = testEntry('a-clip.mp4', mimeType: 'video/mp4');
  final second = testEntry('b-manual.pdf', mimeType: 'application/pdf');
  final third = testEntry('c-song.mp3', mimeType: 'audio/mpeg');
  final fourth = testEntry('d-data.bin', mimeType: 'application/octet-stream');
  final photo = testEntry('photo.heic', mimeType: 'image/heic', hasPreview: true, previewId: 'v1');
  final entries = [first, second, third, fourth, photo];
  final pixel = base64Decode(
    'iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mNkYAAAAAYAAjCB0C8AAAAASUVORK5CYII=',
  );

  late MockFilesService files;
  late MockBrowserFiles browser;
  late FakeBrowserEmbeds embeds;

  setUpAll(() {
    registerFallbackValue(PreviewSize.thumbnail);
    registerFallbackValue(CancelToken());
  });

  setUp(() {
    files = stubFiles(
      folders: {
        'root': testFolder(testRoot, entries),
        testRootId: testFolder(testRoot, entries),
        for (final entry in entries) entry.id: testFolder(entry, const []),
      },
    );
    when(
      () => files.viewUri(any()),
    ).thenAnswer((invocation) => Uri.parse('/view/${invocation.positionalArguments.single}'));
    when(
      () => files.downloadUri(any()),
    ).thenAnswer((invocation) => Uri.parse('/get/${invocation.positionalArguments.single}'));
    when(() => files.preview(any(), any(), any(), any())).thenAnswer((_) async => pixel);
    browser = stubBrowser();
    embeds = FakeBrowserEmbeds();
  });

  Future<ProviderContainer> pumpViewer(WidgetTester tester, String id, {Size size = const Size(1280, 800)}) =>
      pumpRoutedApp(
        tester,
        location: '/view/$id',
        size: size,
        overrides: serviceOverrides(auth: MockAuthService(), files: files, browser: browser, embeds: embeds),
      );

  testWidgets('plays a video with the name in the top bar', (tester) async {
    await pumpViewer(tester, first.id);

    expect(find.byType(ViewerPage), findsOneWidget);
    expect(find.text(first.name), findsOneWidget);
    expect(find.text('video /view/${first.id}'), findsOneWidget);
  });

  testWidgets('shows a PDF in a document frame in portrait', (tester) async {
    await pumpViewer(tester, second.id, size: portrait);

    expect(find.text('document /view/${second.id}'), findsOneWidget);
  });

  testWidgets('shows an audio player', (tester) async {
    await pumpViewer(tester, third.id);

    expect(find.text('audio /view/${third.id}'), findsOneWidget);
  });

  testWidgets('shows the large preview of an image that the browser cannot decode', (tester) async {
    await pumpViewer(tester, photo.id);

    expect(find.byType(InteractiveViewer), findsOneWidget);
    verify(() => files.preview(photo.id, PreviewSize.large, 'v1', any())).called(1);
    verifyNever(() => files.original(any(), any()));
  });

  testWidgets('offers a download for a file that it cannot show', (tester) async {
    await pumpViewer(tester, fourth.id, size: portrait);

    expect(find.text('Guardou cannot show this file.'), findsOneWidget);
    await tester.tap(find.widgetWithText(FilledButton, 'Download'));

    verify(() => browser.download(Uri.parse('/get/${fourth.id}'))).called(1);
  });

  testWidgets('steps through the media files of the folder only', (tester) async {
    final container = await pumpViewer(tester, first.id);
    expect(find.byTooltip('Previous file'), findsNothing);

    await tester.tap(find.byTooltip('Next file'));
    await tester.pumpAndSettle();
    expect(container.read(appRouterProvider).state.uri.path, '/view/${third.id}');

    await tester.tap(find.byTooltip('Next file'));
    await tester.pumpAndSettle();
    expect(container.read(appRouterProvider).state.uri.path, '/view/${photo.id}');
    expect(find.byTooltip('Next file'), findsNothing);

    await tester.tap(find.byTooltip('Previous file'));
    await tester.pumpAndSettle();
    expect(container.read(appRouterProvider).state.uri.path, '/view/${third.id}');
  });

  testWidgets('does not step from a document', (tester) async {
    await pumpViewer(tester, second.id);

    expect(find.byTooltip('Previous file'), findsNothing);
    expect(find.byTooltip('Next file'), findsNothing);
  });

  testWidgets('closes to the file in its folder', (tester) async {
    final container = await pumpViewer(tester, first.id);

    await tester.tap(find.byTooltip('Close'));
    await tester.pumpAndSettle();

    expect(container.read(appRouterProvider).state.uri.path, '/file/${first.id}');
  });
}
