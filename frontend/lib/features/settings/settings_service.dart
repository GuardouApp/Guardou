import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../api/generated/api_client.dart';
import '../../api/generated/models/task_concurrency.dart';
import '../../core/api/api_providers.dart';

final settingsServiceProvider = Provider<SettingsService>((ref) => SettingsService(ref.watch(apiClientProvider)));

class SettingsService {
  const SettingsService(this._api);

  final ApiClient _api;

  Future<List<TaskConcurrency>> taskSettings() => _api.settings.getTaskSettings();

  Future<List<TaskConcurrency>> changeTaskSettings(List<TaskConcurrency> settings) =>
      _api.settings.changeTaskSettings(body: settings);
}
