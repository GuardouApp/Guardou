import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:guardou/core/api/api_providers.dart';
import 'package:guardou/core/widgets/user_avatar.dart';

void main() {
  Future<void> pumpAvatar(WidgetTester tester, {required bool hasPicture}) => tester.pumpWidget(
    ProviderScope(
      overrides: [apiBaseUrlProvider.overrideWithValue('https://guardou.example.com')],
      child: MaterialApp(
        home: UserAvatar(userId: 'alice-id', name: 'alice Smith', hasPicture: hasPicture, radius: 16),
      ),
    ),
  );

  testWidgets('loads the picture of the user', (tester) async {
    await pumpAvatar(tester, hasPicture: true);

    final image = tester.widget<Image>(find.byType(Image)).image as NetworkImage;
    expect(image.url, 'https://guardou.example.com/api/users/alice-id/picture');
  });

  testWidgets('shows the initial of the display name without a picture', (tester) async {
    await pumpAvatar(tester, hasPicture: false);

    expect(find.byType(Image), findsNothing);
    expect(find.text('A'), findsOneWidget);
  });
}
