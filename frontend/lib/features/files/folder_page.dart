import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../api/generated/models/item_entry.dart';
import '../../app/router/routes.dart';
import 'folder_controller.dart';
import 'folder_header.dart';
import 'folder_states.dart';
import 'item_browser.dart';
import 'item_open.dart';

class FolderPage extends ConsumerWidget {
  const FolderPage({super.key, required this.folderId, this.focusedItemId});

  final String folderId;
  final String? focusedItemId;

  bool get _recordsOpen => focusedItemId == null && folderId != Routes.rootFolderId;

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    if (_recordsOpen) {
      ref.watch(itemOpenProvider(folderId));
    }
    final folderProvider = folderControllerProvider(folderId);
    final folder = ref.watch(folderProvider).value;
    return ItemBrowser(
      scope: folderId,
      layoutKey: folder?.entry.id ?? folderId,
      items: folderProvider.select((state) => state.whenData((details) => details.items ?? const <ItemEntry>[])),
      header: FolderHeader(folder: folder),
      empty: const EmptyFolderView(),
      errorView: (error) => FolderErrorView(
        isRoot: folderId == Routes.rootFolderId,
        error: error,
        onRetry: () => ref.invalidate(folderProvider),
      ),
      folder: folder?.entry,
      focusedItemId: focusedItemId,
    );
  }
}
