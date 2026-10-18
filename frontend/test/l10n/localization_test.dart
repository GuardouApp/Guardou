import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:guardou/features/auth/login_page.dart';
import 'package:guardou/l10n/generated/app_localizations.dart';

import '../helpers/test_app.dart';

void main() {
  test('supports English and Brazilian Portuguese', () {
    expect(AppLocalizations.supportedLocales, containsAll([const Locale('en'), const Locale('pt', 'BR')]));
  });

  testWidgets('shows the login page in Brazilian Portuguese', (tester) async {
    await pumpTestApp(
      tester,
      locale: const Locale('pt', 'BR'),
      overrides: serviceOverrides(auth: MockAuthService(), user: null),
      child: const LoginPage(),
    );

    expect(find.text('Entrar no Guardou'), findsOneWidget);
    expect(find.text('Manter conectado'), findsOneWidget);
  });
}
