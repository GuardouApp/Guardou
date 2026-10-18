import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../api/generated/models/task_concurrency.dart';
import '../../core/providers/retry.dart';
import 'settings_service.dart';

final taskSettingsProvider = AsyncNotifierProvider.autoDispose<TaskSettingsController, List<TaskConcurrency>>(
  TaskSettingsController.new,
  retry: noRetry,
);

class TaskSettingsController extends AsyncNotifier<List<TaskConcurrency>> {
  @override
  Future<List<TaskConcurrency>> build() => ref.read(settingsServiceProvider).taskSettings();

  Future<void> save(List<TaskConcurrency> settings) async {
    state = AsyncData(await ref.read(settingsServiceProvider).changeTaskSettings(settings));
  }
}
