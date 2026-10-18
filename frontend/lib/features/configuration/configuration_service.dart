import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../api/generated/api_client.dart';
import '../../api/generated/models/runtime_configuration.dart';
import '../../core/api/api_providers.dart';

final configurationServiceProvider = Provider<ConfigurationService>(
  (ref) => ConfigurationService(ref.watch(apiClientProvider)),
);

class ConfigurationService {
  const ConfigurationService(this._api);

  final ApiClient _api;

  Future<RuntimeConfiguration> load() => _api.configuration.getConfiguration();
}
