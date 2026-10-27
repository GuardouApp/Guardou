import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:guardou/api/generated/models/item_kind.dart';
import 'package:guardou/app/router/routes.dart';
import 'package:mocktail/mocktail.dart';

import '../../helpers/test_app.dart';

void main() {
  const portrait = Size(390, 844);

  final photos = testEntry('photos', kind: ItemKind.folder);
  final zeta = testEntry('zeta.txt');
  final alpha = testEntry('alpha.txt');

  late MockFilesService files;

  setUp(() {
    files = stubFiles();
    when(files.recent).thenAnswer((_) async => [photos, zeta, alpha]);
  });

  Future<ProviderContainer> pumpHome(WidgetTester tester, {Size size = const Size(1280, 800)}) => pumpRoutedApp(
    tester,
    location: Routes.home,
    size: size,
    overrides: serviceOverrides(auth: MockAuthService(), files: files),
  );

  testWidgets('lists the recent folders and files in the server order', (tester) async {
    await pumpHome(tester);

    expect(find.text('Home'), findsWidgets);
    expect(tester.getTopLeft(find.text('photos')).dy, lessThan(tester.getTopLeft(find.text('zeta.txt')).dy));
    expect(tester.getTopLeft(find.text('zeta.txt')).dy, lessThan(tester.getTopLeft(find.text('alpha.txt')).dy));
  });

  testWidgets('shows an empty state in portrait', (tester) async {
    when(files.recent).thenAnswer((_) async => []);

    await pumpHome(tester, size: portrait);

    expect(find.text('No recent items'), findsOneWidget);
    expect(find.text('Folders and files that you add or open appear here.'), findsOneWidget);
  });
}
