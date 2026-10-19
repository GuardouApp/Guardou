import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../api/generated/models/item_entry.dart';
import '../../api/generated/models/item_kind.dart';
import '../files/file_arrangement.dart';
import '../files/file_view_settings.dart';
import '../files/folder_controller.dart';

final viewerSiblingsProvider = Provider.autoDispose.family<List<ItemEntry>, String>((ref, folderId) {
  final children = ref.watch(childrenControllerProvider(folderId)).value ?? const [];
  return arrangeEntries([
    for (final child in children)
      if (child.kind != ItemKind.folder) child,
  ], ref.watch(fileViewSettingsProvider));
});
