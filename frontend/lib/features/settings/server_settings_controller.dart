import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../api/generated/models/server_settings.dart';
import '../../core/providers/retry.dart';
import 'oidc_settings_controller.dart';
import 'settings_service.dart';

final serverSettingsProvider = AsyncNotifierProvider.autoDispose<ServerSettingsController, ServerSettings>(
  ServerSettingsController.new,
  retry: noRetry,
);

class ServerSettingsController extends AsyncNotifier<ServerSettings> {
  @override
  Future<ServerSettings> build() => ref.read(settingsServiceProvider).serverSettings();

  Future<ServerSettings> defaults() => ref.read(settingsServiceProvider).serverSettingsDefaults();

  Future<void> save(ServerSettings settings) async {
    state = AsyncData(await ref.read(settingsServiceProvider).changeServerSettings(settings));
    ref.invalidate(oidcSettingsProvider);
  }
}
