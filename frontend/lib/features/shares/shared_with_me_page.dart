import 'dart:async';

import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:lucide_icons_flutter/lucide_icons.dart';

import '../../api/generated/models/item_entry.dart';
import '../../app/router/routes.dart';
import '../../core/widgets/page_header.dart';
import '../../l10n/generated/app_localizations.dart';
import '../files/drag_select_area.dart';
import '../files/file_actions.dart';
import '../files/file_arrangement.dart';
import '../files/file_grid_view.dart';
import '../files/file_list_view.dart';
import '../files/file_view_settings.dart';
import '../files/files_service.dart';
import '../files/folder_header.dart';
import '../files/folder_layouts.dart';
import '../files/folder_states.dart';
import '../files/item_action.dart';
import '../files/item_actions_menu.dart';
import '../files/item_interactions.dart';
import '../files/selection.dart';
import 'shared_with_me.dart';

class SharedWithMePage extends ConsumerStatefulWidget {
  const SharedWithMePage({super.key});

  static const layoutKey = Routes.shared;
  static const _menuActions = [ItemAction.open, ItemAction.download];

  @override
  ConsumerState<SharedWithMePage> createState() => _SharedWithMePageState();
}

class _SharedWithMePageState extends ConsumerState<SharedWithMePage> {
  final _keys = ItemKeyRegistry();

  Future<void> _showActions(ItemEntry entry, Offset position) async {
    final action = await showItemActions(
      context,
      entries: [entry],
      position: position,
      actions: SharedWithMePage._menuActions,
    );
    if (action != null && mounted) {
      await FileActions(context).perform(action, [entry]);
    }
  }

  ItemInteractions _interactions() => ItemInteractions(
    selection: Selection.empty,
    openOnTap: true,
    onTap: (entry) => unawaited(FileActions(context).open(entry)),
    onOpen: (entry) => unawaited(FileActions(context).open(entry)),
    onLongPress: (_) {},
    onContextMenu: (entry, position) => unawaited(_showActions(entry, position)),
    onMore: (entry, anchor) => unawaited(_showActions(entry, anchorPosition(anchor))),
    keyFor: _keys.keyFor,
    decorate: (_, child) => child,
    source: ref.read(filesServiceProvider),
  );

  Widget _contents(List<ItemEntry> items) {
    final l10n = AppLocalizations.of(context);
    if (items.isEmpty) {
      return FolderMessage(
        icon: LucideIcons.users,
        title: l10n.sharedWithMeEmptyTitle,
        message: l10n.sharedWithMeEmptyMessage,
      );
    }
    final entries = arrangeEntries(items, ref.watch(fileViewSettingsProvider));
    final layout = ref.watch(folderLayoutsProvider.select((layouts) => layouts.of(SharedWithMePage.layoutKey)));
    final scrollKey = '${SharedWithMePage.layoutKey}:${layout.name}';
    final interactions = _interactions();
    return switch (layout) {
      FileLayout.list => FileListView(entries: entries, interactions: interactions, scrollKey: scrollKey),
      FileLayout.grid => FileGridView(entries: entries, interactions: interactions, scrollKey: scrollKey),
    };
  }

  @override
  Widget build(BuildContext context) {
    final l10n = AppLocalizations.of(context);
    return Column(
      crossAxisAlignment: CrossAxisAlignment.stretch,
      children: [
        PageHeader(
          child: Row(
            children: [
              Expanded(child: Text(l10n.navigationSharedWithMe)),
              const FolderLayoutToggle(folderId: SharedWithMePage.layoutKey),
            ],
          ),
        ),
        Expanded(
          child: switch (ref.watch(sharedWithMeProvider)) {
            AsyncValue(hasValue: true, :final value?) => _contents(value),
            AsyncValue(:final error?) => FolderErrorView(
              isRoot: true,
              error: error,
              onRetry: () => ref.invalidate(sharedWithMeProvider),
            ),
            _ => Center(child: CircularProgressIndicator(semanticsLabel: l10n.loading)),
          },
        ),
      ],
    );
  }
}
