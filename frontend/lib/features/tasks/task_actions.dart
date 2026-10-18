import 'package:dio/dio.dart';
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../api/generated/models/task_entry.dart';
import '../../api/generated/models/task_kind.dart';
import '../../api/generated/models/task_state.dart';
import '../../l10n/generated/app_localizations.dart';
import '../files/dialogs/confirm_dialog.dart';
import 'tasks_controller.dart';
import 'tasks_service.dart';

class TaskActions {
  TaskActions(this._context)
    : _container = ProviderScope.containerOf(_context, listen: false),
      _messenger = ScaffoldMessenger.of(_context),
      _l10n = AppLocalizations.of(_context);

  static const _conflict = 409;

  final BuildContext _context;
  final ProviderContainer _container;
  final ScaffoldMessengerState _messenger;
  final AppLocalizations _l10n;

  TasksService get _service => _container.read(tasksServiceProvider);

  Future<void> run(TaskEntry task) => _perform(() async {
    final result = await _service.run(task.id);
    return result?.state == TaskState.failed ? _l10n.taskFailed : _l10n.taskCompleted;
  });

  Future<void> reset(TaskEntry task) => _perform(() async {
    await _service.reset(task.id);
    return _l10n.taskResetDone;
  });

  Future<void> resetFailed(TaskKind kind) => _perform(() async {
    await _service.resetFailed(kind);
    return _l10n.tasksResetDone;
  });

  Future<void> queueAll(TaskKind kind) async {
    final confirmed = await showConfirmDialog(
      _context,
      title: _l10n.queueAllTitle,
      message: _l10n.queueAllMessage,
      confirmLabel: _l10n.queueAll,
    );
    if (confirmed) {
      await _perform(() async {
        await _service.queueAll(kind);
        return _l10n.queueAllDone;
      });
    }
  }

  Future<void> _perform(Future<String> Function() operation) async {
    try {
      _show(await operation());
    } on DioException catch (error) {
      _show(error.response?.statusCode == _conflict ? _l10n.taskBusy : _l10n.genericError);
    } finally {
      _container
        ..invalidate(taskQueuesProvider)
        ..invalidate(taskListProvider);
    }
  }

  void _show(String message) => _messenger
    ..hideCurrentSnackBar()
    ..showSnackBar(SnackBar(content: Text(message)));
}
