import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../features/configuration/configuration_controller.dart';
import '../l10n/generated/app_localizations.dart';
import 'browser_chrome.dart';
import 'router/app_router.dart';
import 'theme/app_theme.dart';

class App extends ConsumerWidget {
  const App({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final applicationName = ref.watch(
      configurationControllerProvider.select((configuration) => configuration.value?.applicationName),
    );

    return MaterialApp.router(
      title: applicationName ?? '',
      theme: AppTheme.light(),
      darkTheme: AppTheme.dark(),
      localizationsDelegates: AppLocalizations.localizationsDelegates,
      supportedLocales: AppLocalizations.supportedLocales,
      routerConfig: ref.watch(appRouterProvider),
      builder: (context, child) => BrowserChrome(child: child ?? const SizedBox.shrink()),
    );
  }
}
