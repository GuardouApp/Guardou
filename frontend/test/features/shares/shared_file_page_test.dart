import 'package:dio/dio.dart';
import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:guardou/features/shares/shared_file_source.dart';
import 'package:mocktail/mocktail.dart';

import '../../helpers/test_app.dart';

void main() {
  const token = 'AAAAAAAAAAAAAAAAAAAAAA';
  const portrait = Size(390, 844);
  final file = testEntry('clip.mp4', id: 'file-id', parentId: null, canModify: false, mimeType: 'video/mp4');

  late MockSharesService shares;
  late MockAuthService auth;

  setUp(() {
    shares = MockSharesService();
    auth = MockAuthService();
    when(() => shares.files(token)).thenReturn(SharedFileSource(Dio(), Uri.parse('http://localhost'), token));
  });

  Future<void> pumpShare(WidgetTester tester, {Size size = const Size(1280, 800)}) => pumpRoutedApp(
    tester,
    location: '/s/$token',
    size: size,
    overrides: serviceOverrides(auth: auth, user: null, shares: shares),
  );

  testWidgets('shows the shared file without a session', (tester) async {
    when(() => shares.sharedItem(token)).thenAnswer((_) async => file);

    await pumpShare(tester);

    expect(find.text(file.name), findsOneWidget);
    expect(find.text('video http://localhost/api/shares/$token/items/${file.id}/view'), findsOneWidget);
  });

  testWidgets('shows the shared file in portrait', (tester) async {
    when(() => shares.sharedItem(token)).thenAnswer((_) async => file);

    await pumpShare(tester, size: portrait);

    expect(find.text(file.name), findsOneWidget);
  });

  testWidgets('tells that an unknown link is not available', (tester) async {
    when(() => shares.sharedItem(token)).thenThrow(testNotFound('/api/shares/$token'));

    await pumpShare(tester);

    expect(find.text('This link does not exist, or the owner stopped sharing the file.'), findsOneWidget);
  });
}
