import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../api/generated/models/oidc_settings_change.dart';
import '../../api/generated/models/oidc_settings_view.dart';
import '../../core/providers/retry.dart';
import '../configuration/configuration_controller.dart';
import 'settings_service.dart';

final oidcSettingsProvider = AsyncNotifierProvider.autoDispose<OidcSettingsController, OidcSettingsView>(
  OidcSettingsController.new,
  retry: noRetry,
);

class OidcSettingsController extends AsyncNotifier<OidcSettingsView> {
  @override
  Future<OidcSettingsView> build() => ref.read(settingsServiceProvider).oidcSettings();

  Future<OidcSettingsView> defaults() => ref.read(settingsServiceProvider).oidcSettingsDefaults();

  Future<void> save(OidcSettingsChange settings) async {
    state = AsyncData(await ref.read(settingsServiceProvider).changeOidcSettings(settings));
    await ref.read(configurationControllerProvider.notifier).refresh();
  }
}
