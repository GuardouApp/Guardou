import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../api/generated/models/task_entry.dart';
import '../../api/generated/models/task_kind.dart';
import '../../api/generated/models/task_queue_summary.dart';
import '../../api/generated/models/task_state.dart';
import '../../core/providers/retry.dart';
import 'tasks_service.dart';

final taskQueuesProvider = AsyncNotifierProvider.autoDispose<TaskQueuesController, List<TaskQueueSummary>>(
  TaskQueuesController.new,
  retry: noRetry,
);

class TaskQueuesController extends AsyncNotifier<List<TaskQueueSummary>> {
  @override
  Future<List<TaskQueueSummary>> build() => ref.read(tasksServiceProvider).queues();
}

typedef TaskListKey = ({TaskKind kind, TaskState state});

final taskListProvider = FutureProvider.autoDispose.family<List<TaskEntry>, TaskListKey>(
  (ref, key) => ref.read(tasksServiceProvider).list(key.kind, key.state),
  retry: noRetry,
);
