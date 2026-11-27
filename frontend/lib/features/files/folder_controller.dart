import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../api/generated/models/item_details.dart';
import '../../api/generated/models/item_entry.dart';
import '../../core/providers/retry.dart';
import '../realtime/item_changes.dart';
import 'files_service.dart';

final folderControllerProvider = AsyncNotifierProvider.autoDispose.family<FolderController, ItemDetails, String>(
  FolderController.new,
  retry: noRetry,
);

final fileDetailsProvider = FutureProvider.autoDispose.family<ItemDetails, String>(
  (ref, id) => ref.read(filesServiceProvider).item(id),
  retry: noRetry,
);

final childrenControllerProvider = AsyncNotifierProvider.autoDispose
    .family<ChildrenController, List<ItemEntry>, String>(ChildrenController.new, retry: noRetry);

class FolderController extends AsyncNotifier<ItemDetails> {
  FolderController(this.folderId);

  final String folderId;

  @override
  Future<ItemDetails> build() {
    refreshOnItemChanges(ref, () => [folderId, state.value?.entry.id]);
    return ref.read(filesServiceProvider).item(folderId);
  }
}

class ChildrenController extends AsyncNotifier<List<ItemEntry>> {
  ChildrenController(this.folderId);

  final String folderId;

  @override
  Future<List<ItemEntry>> build() {
    refreshOnItemChanges(ref, () => [folderId, state.value?.firstOrNull?.parentId]);
    return ref.read(filesServiceProvider).children(folderId);
  }
}
