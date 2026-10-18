import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../api/generated/models/trash_entry.dart';
import '../../core/providers/retry.dart';
import 'trash_service.dart';

final trashControllerProvider = AsyncNotifierProvider.autoDispose<TrashController, List<TrashEntry>>(
  TrashController.new,
  retry: noRetry,
);

class TrashController extends AsyncNotifier<List<TrashEntry>> {
  @override
  Future<List<TrashEntry>> build() => ref.read(trashServiceProvider).list();
}
