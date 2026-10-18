import 'package:flutter/foundation.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

@immutable
class OperationStatus {
  const OperationStatus({required this.id, required this.label, this.progress});

  final int id;
  final String label;
  final double? progress;

  OperationStatus withProgress(double progress) => OperationStatus(id: id, label: label, progress: progress);
}

final operationStatusProvider = NotifierProvider<OperationStatusController, List<OperationStatus>>(
  OperationStatusController.new,
);

class OperationStatusController extends Notifier<List<OperationStatus>> {
  int _nextId = 0;

  @override
  List<OperationStatus> build() => const [];

  Future<T> track<T>(String label, Future<T> Function(void Function(double progress) report) operation) async {
    final id = _nextId++;
    state = [...state, OperationStatus(id: id, label: label)];
    try {
      return await operation((progress) => _report(id, progress));
    } finally {
      state = state.where((status) => status.id != id).toList();
    }
  }

  void _report(int id, double progress) =>
      state = [for (final status in state) status.id == id ? status.withProgress(progress) : status];
}
