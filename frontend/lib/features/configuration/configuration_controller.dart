import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../api/generated/models/runtime_configuration.dart';
import 'configuration_service.dart';

final configurationControllerProvider = AsyncNotifierProvider<ConfigurationController, RuntimeConfiguration>(
  ConfigurationController.new,
);

class ConfigurationController extends AsyncNotifier<RuntimeConfiguration> {
  @override
  Future<RuntimeConfiguration> build() => ref.read(configurationServiceProvider).load();

  Future<void> refresh() async {
    final configuration = await AsyncValue.guard(ref.read(configurationServiceProvider).load);
    if (configuration.hasValue) {
      state = configuration;
    }
  }
}
