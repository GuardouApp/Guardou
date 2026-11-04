import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../api/generated/models/user_preferences.dart';
import '../../core/providers/retry.dart';
import '../auth/session_controller.dart';
import 'preferences_service.dart';

final preferencesControllerProvider = AsyncNotifierProvider<PreferencesController, UserPreferences?>(
  PreferencesController.new,
  retry: noRetry,
);

class PreferencesController extends AsyncNotifier<UserPreferences?> {
  @override
  Future<UserPreferences?> build() async {
    final userId = ref.watch(sessionControllerProvider.select((session) => session.value?.id));
    return userId == null ? null : ref.read(preferencesServiceProvider).preferences();
  }

  Future<UserPreferences> defaults() => ref.read(preferencesServiceProvider).defaults();

  Future<void> save(UserPreferences preferences) async {
    state = AsyncData(await ref.read(preferencesServiceProvider).change(preferences));
  }
}
