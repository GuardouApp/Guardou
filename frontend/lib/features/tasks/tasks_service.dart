import 'package:dio/dio.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../api/generated/api_client.dart';
import '../../api/generated/models/task_entry.dart';
import '../../api/generated/models/task_kind.dart';
import '../../api/generated/models/task_queue_summary.dart';
import '../../api/generated/models/task_state.dart';
import '../../core/api/api_providers.dart';

final tasksServiceProvider = Provider<TasksService>(
  (ref) => TasksService(ref.watch(apiClientProvider), ref.watch(dioProvider)),
);

class TasksService {
  const TasksService(this._api, this._dio);

  final ApiClient _api;
  final Dio _dio;

  Future<List<TaskQueueSummary>> queues() => _api.tasks.listTaskQueues();

  Future<List<TaskEntry>> list(TaskKind kind, TaskState state) => _api.tasks.listTasks(kind: kind, state: state);

  Future<TaskEntry?> run(String id) async {
    final response = await _dio.post<Map<String, Object?>>('/api/tasks/${Uri.encodeComponent(id)}/run');
    final data = response.data;
    return data == null ? null : TaskEntry.fromJson(data);
  }

  Future<TaskEntry> reset(String id) => _api.tasks.resetTask(id: id);

  Future<void> resetFailed(TaskKind kind) => _api.tasks.resetFailedTasks(kind: kind);

  Future<void> queueAll(TaskKind kind) => _api.tasks.queueAllTasks(kind: kind);
}
