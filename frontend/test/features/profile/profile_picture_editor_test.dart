import 'dart:typed_data';

import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:guardou/api/generated/models/current_user.dart';
import 'package:guardou/core/api/api_providers.dart';
import 'package:guardou/core/browser/browser_files.dart';
import 'package:guardou/features/profile/profile_picture_failure.dart';
import 'package:guardou/features/shell/account_menu.dart';
import 'package:mocktail/mocktail.dart';

import '../../helpers/test_app.dart';

void main() {
  late MockAuthService auth;
  late MockProfilePictureService pictures;
  late MockBrowserFiles browser;

  final picture = LocalFile(name: 'me.png', size: 3, read: () async => Uint8List.fromList([1, 2, 3]));
  final userWithPicture = testUser.copyWith(hasPicture: true);

  setUpAll(() => registerFallbackValue(picture));

  setUp(() {
    auth = MockAuthService();
    pictures = MockProfilePictureService();
    browser = stubBrowser();
  });

  Future<void> openPanel(WidgetTester tester, Size size, {CurrentUser user = testUser}) async {
    await pumpTestApp(
      tester,
      size: size,
      overrides: [
        ...serviceOverrides(auth: auth, user: user, pictures: pictures, browser: browser),
        apiBaseUrlProvider.overrideWithValue('https://guardou.example.com'),
      ],
      child: const Scaffold(
        body: Align(alignment: Alignment.topRight, child: AccountMenu()),
      ),
    );
    await tester.tap(find.byTooltip('Account'));
    await tester.pumpAndSettle();
  }

  for (final size in [const Size(1280, 800), const Size(390, 844)]) {
    testWidgets('uploads a picture when the user has none at $size', (tester) async {
      when(browser.pickImage).thenAnswer((_) async => picture);
      when(() => pictures.replace(any(), any())).thenAnswer((_) async {});
      await openPanel(tester, size);
      when(auth.restore).thenAnswer((_) async => userWithPicture);

      await tester.tap(find.byTooltip('Change the profile picture'));
      await tester.pumpAndSettle();

      verify(() => pictures.replace(testUser.id, picture)).called(1);
      expect(find.text('Profile picture changed'), findsOneWidget);
    });
  }

  testWidgets('removes the picture from the menu', (tester) async {
    when(() => pictures.delete(any())).thenAnswer((_) async {});
    await openPanel(tester, const Size(1280, 800), user: userWithPicture);
    when(auth.restore).thenAnswer((_) async => testUser);

    await tester.tap(find.byTooltip('Change the profile picture'));
    await tester.pumpAndSettle();
    await tester.tap(find.text('Remove the picture'));
    await tester.pumpAndSettle();

    verify(() => pictures.delete(testUser.id)).called(1);
    expect(find.text('Profile picture removed'), findsOneWidget);
  });

  testWidgets('refuses a picture that is too large', (tester) async {
    final large = LocalFile(name: 'large.png', size: maximumPictureBytes + 1, read: () async => Uint8List(0));
    when(browser.pickImage).thenAnswer((_) async => large);
    await openPanel(tester, const Size(1280, 800));

    await tester.tap(find.byTooltip('Change the profile picture'));
    await tester.pumpAndSettle();

    verifyNever(() => pictures.replace(any(), any()));
    expect(find.text('The picture is too large. The maximum is 5 MB.'), findsOneWidget);
  });

  testWidgets('shows a message when the server refuses the picture', (tester) async {
    when(browser.pickImage).thenAnswer((_) async => picture);
    when(() => pictures.replace(any(), any())).thenThrow(testProblem('/api/users/admin-id/picture', status: 400));
    await openPanel(tester, const Size(1280, 800));

    await tester.tap(find.byTooltip('Change the profile picture'));
    await tester.pumpAndSettle();

    expect(find.text('The file is not a supported image.'), findsOneWidget);
  });
}
