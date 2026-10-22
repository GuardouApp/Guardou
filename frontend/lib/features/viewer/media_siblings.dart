import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../api/generated/models/item_entry.dart';
import '../files/file_arrangement.dart';
import '../files/file_view_settings.dart';
import '../files/folder_controller.dart';
import 'viewer_kind.dart';

final mediaSiblingsProvider = Provider.autoDispose.family<List<ItemEntry>, String>(
  (ref, folderId) => mediaEntries(
    ref.watch(childrenControllerProvider(folderId)).value ?? const [],
    ref.watch(fileViewSettingsProvider),
  ),
);

List<ItemEntry> mediaEntries(List<ItemEntry> children, FileViewSettings settings) => arrangeEntries([
  for (final child in children)
    if (ViewerKind.of(child).isMedia) child,
], settings);
