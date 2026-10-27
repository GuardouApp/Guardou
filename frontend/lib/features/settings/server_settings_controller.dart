import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../api/generated/models/server_settings.dart';
import '../../api/generated/models/server_settings_view.dart';
import '../../core/providers/retry.dart';
import '../configuration/configuration_controller.dart';
import 'oidc_settings_controller.dart';
import 'settings_service.dart';

final serverSettingsProvider = AsyncNotifierProvider.autoDispose<ServerSettingsController, ServerSettingsView>(
  ServerSettingsController.new,
  retry: noRetry,
);

class ServerSettingsController extends AsyncNotifier<ServerSettingsView> {
  @override
  Future<ServerSettingsView> build() => ref.read(settingsServiceProvider).serverSettings();

  Future<ServerSettingsView> defaults() => ref.read(settingsServiceProvider).serverSettingsDefaults();

  Future<void> save(ServerSettings settings) async {
    state = AsyncData(await ref.read(settingsServiceProvider).changeServerSettings(settings));
    ref.invalidate(oidcSettingsProvider);
    await ref.read(configurationControllerProvider.notifier).refresh();
  }
}
