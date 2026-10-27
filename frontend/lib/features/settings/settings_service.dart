import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../api/generated/api_client.dart';
import '../../api/generated/models/oidc_settings_change.dart';
import '../../api/generated/models/oidc_settings_view.dart';
import '../../api/generated/models/server_settings.dart';
import '../../api/generated/models/server_settings_view.dart';
import '../../api/generated/models/task_concurrency.dart';
import '../../core/api/api_providers.dart';

final settingsServiceProvider = Provider<SettingsService>((ref) => SettingsService(ref.watch(apiClientProvider)));

class SettingsService {
  const SettingsService(this._api);

  final ApiClient _api;

  Future<List<TaskConcurrency>> taskSettings() => _api.settings.getTaskSettings();

  Future<List<TaskConcurrency>> taskSettingsDefaults() => _api.settings.getTaskSettingsDefaults();

  Future<List<TaskConcurrency>> changeTaskSettings(List<TaskConcurrency> settings) =>
      _api.settings.changeTaskSettings(body: settings);

  Future<OidcSettingsView> oidcSettings() => _api.settings.getOidcSettings();

  Future<OidcSettingsView> oidcSettingsDefaults() => _api.settings.getOidcSettingsDefaults();

  Future<OidcSettingsView> changeOidcSettings(OidcSettingsChange settings) =>
      _api.settings.changeOidcSettings(body: settings);

  Future<ServerSettingsView> serverSettings() => _api.settings.getServerSettings();

  Future<ServerSettingsView> serverSettingsDefaults() => _api.settings.getServerSettingsDefaults();

  Future<ServerSettingsView> changeServerSettings(ServerSettings settings) =>
      _api.settings.changeServerSettings(body: settings);
}
