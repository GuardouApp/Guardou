import 'dart:async';

import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../api/generated/models/item_entry.dart';
import '../../core/widgets/page_header.dart';
import '../../l10n/generated/app_localizations.dart';
import 'drag_select_area.dart';
import 'file_actions.dart';
import 'file_grid_view.dart';
import 'file_list_view.dart';
import 'files_service.dart';
import 'folder_header.dart';
import 'folder_layouts.dart';
import 'folder_states.dart';
import 'item_action.dart';
import 'item_actions_menu.dart';
import 'item_interactions.dart';
import 'selection.dart';

class ItemCollectionPage extends ConsumerStatefulWidget {
  const ItemCollectionPage({
    super.key,
    required this.title,
    required this.layoutKey,
    required this.items,
    required this.arrange,
    required this.onRetry,
    required this.empty,
  });

  static const _menuActions = [ItemAction.open, ItemAction.download];

  final String title;
  final String layoutKey;
  final AsyncValue<List<ItemEntry>> items;
  final List<ItemEntry> Function(List<ItemEntry> items) arrange;
  final VoidCallback onRetry;
  final FolderMessage empty;

  @override
  ConsumerState<ItemCollectionPage> createState() => _ItemCollectionPageState();
}

class _ItemCollectionPageState extends ConsumerState<ItemCollectionPage> {
  final _keys = ItemKeyRegistry();

  Future<void> _showActions(ItemEntry entry, Offset position) async {
    final action = await showItemActions(
      context,
      entries: [entry],
      position: position,
      actions: ItemCollectionPage._menuActions,
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
    if (items.isEmpty) {
      return widget.empty;
    }
    final entries = widget.arrange(items);
    final layout = ref.watch(folderLayoutsProvider.select((layouts) => layouts.of(widget.layoutKey)));
    final scrollKey = '${widget.layoutKey}:${layout.name}';
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
              Expanded(child: Text(widget.title)),
              FolderLayoutToggle(folderId: widget.layoutKey),
            ],
          ),
        ),
        Expanded(
          child: switch (widget.items) {
            AsyncValue(hasValue: true, :final value?) => _contents(value),
            AsyncValue(:final error?) => FolderErrorView(isRoot: true, error: error, onRetry: widget.onRetry),
            _ => Center(child: CircularProgressIndicator(semanticsLabel: l10n.loading)),
          },
        ),
      ],
    );
  }
}
