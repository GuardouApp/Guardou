import 'dart:async';

import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../api/generated/models/item_details.dart';
import '../../api/generated/models/item_entry.dart';
import '../../api/generated/models/item_kind.dart';
import '../../app/router/routes.dart';
import '../../core/layout/window_size_class.dart';
import '../../l10n/generated/app_localizations.dart';
import 'details_panel.dart';
import 'details_panel_controller.dart';
import 'drag_select_area.dart';
import 'file_actions.dart';
import 'file_arrangement.dart';
import 'file_drop_target.dart';
import 'file_grid_view.dart';
import 'file_list_view.dart';
import 'file_view_settings.dart';
import 'files_service.dart';
import 'folder_controller.dart';
import 'folder_header.dart';
import 'folder_layouts.dart';
import 'folder_states.dart';
import 'item_action.dart';
import 'item_actions_menu.dart';
import 'item_interactions.dart';
import 'new_item_menu.dart';
import 'selection.dart';
import 'selection_controller.dart';
import 'selection_toolbar.dart';

class FolderPage extends ConsumerStatefulWidget {
  const FolderPage({super.key, required this.folderId, this.focusedItemId});

  static const _panelWidth = 320.0;

  final String folderId;
  final String? focusedItemId;

  @override
  ConsumerState<FolderPage> createState() => _FolderPageState();
}

class _FolderPageState extends ConsumerState<FolderPage> {
  final _keys = ItemKeyRegistry();
  late String? _pendingFocus = widget.focusedItemId;

  SelectionController get _selection => ref.read(selectionControllerProvider(widget.folderId).notifier);

  @override
  void initState() {
    super.initState();
    ref.listenManual(folderControllerProvider(widget.folderId), (_, next) {
      if (next.value case final folder?) {
        _onLoaded(folder);
      }
    }, fireImmediately: true);
  }

  void _onLoaded(ItemDetails folder) {
    final ids = [for (final item in folder.items ?? const <ItemEntry>[]) item.id];
    final focus = _pendingFocus;
    _pendingFocus = null;
    WidgetsBinding.instance.addPostFrameCallback((_) {
      if (!mounted) {
        return;
      }
      _selection.retainOnly(ids);
      if (focus != null && ids.contains(focus)) {
        _selection.select(focus);
      }
    });
  }

  List<ItemEntry> _targetsFor(ItemEntry entry, List<ItemEntry> entries) {
    final selection = ref.read(selectionControllerProvider(widget.folderId));
    if (selection.contains(entry.id)) {
      return entries.where((item) => selection.contains(item.id)).toList();
    }
    _selection.select(entry.id);
    return [entry];
  }

  Future<void> _showActions(List<ItemEntry> targets, Offset position) async {
    final action = await showItemActions(context, entries: targets, position: position);
    if (action != null && mounted) {
      await FileActions(context).perform(action, targets);
    }
  }

  void _tap(ItemEntry entry, List<String> order, Selection selection) {
    final keyboard = HardwareKeyboard.instance;
    if (keyboard.isShiftPressed) {
      _selection.extendTo(entry.id, order);
    } else if (selection.checkboxMode || keyboard.isControlPressed || keyboard.isMetaPressed) {
      _selection.toggle(entry.id);
    } else {
      _selection.select(entry.id);
    }
  }

  ItemInteractions _interactions(List<ItemEntry> entries, Selection selection) {
    final order = [for (final entry in entries) entry.id];
    return ItemInteractions(
      selection: selection,
      openOnTap: WindowSizeClass.of(context) == WindowSizeClass.compact,
      onTap: (entry) => _tap(entry, order, selection),
      onOpen: (entry) => unawaited(FileActions(context).open(entry)),
      onLongPress: (entry) => _selection.startCheckboxMode(entry.id),
      onContextMenu: (entry, position) => unawaited(_showActions(_targetsFor(entry, entries), position)),
      onMore: (entry, anchor) => unawaited(_showActions(_targetsFor(entry, entries), anchorPosition(anchor))),
      keyFor: _keys.keyFor,
      decorate: (entry, child) => entry.kind == ItemKind.folder
          ? FileDropTarget(folderId: entry.id, enabled: entry.permissions.canAddChildren, child: child)
          : child,
      source: ref.read(filesServiceProvider),
    );
  }

  Widget _header(ItemDetails? folder, List<ItemEntry> selected, Selection selection) {
    if (selection.isEmpty) {
      return FolderHeader(folder: folder);
    }
    final l10n = AppLocalizations.of(context);
    const toolbarActions = [
      ItemAction.download,
      ItemAction.share,
      ItemAction.move,
      ItemAction.copy,
      ItemAction.rename,
      ItemAction.trash,
      ItemAction.details,
    ];
    return SelectionToolbar(
      count: selected.length,
      onClear: _selection.clear,
      onMore: (anchor) => unawaited(_showActions(selected, anchorPosition(anchor))),
      actions: [
        for (final action in toolbarActions)
          ToolbarAction(
            icon: action.icon,
            label: action.label(l10n),
            onPressed: action.isEnabledFor(selected)
                ? () => unawaited(FileActions(context).perform(action, selected))
                : null,
          ),
      ],
    );
  }

  Widget _contents(ItemDetails folder, List<ItemEntry> entries, Selection selection, {required double reservedWidth}) {
    final layout = ref.watch(folderLayoutsProvider.select((layouts) => layouts.of(folder.entry.id)));
    final scrollKey = '${folder.entry.id}:${layout.name}';
    final interactions = _interactions(entries, selection);
    final selected = entries.where((entry) => selection.contains(entry.id)).toList();
    void selectAll() => _selection.selectAll([for (final entry in entries) entry.id]);

    return FileDropTarget(
      folderId: folder.entry.id,
      enabled: folder.entry.permissions.canAddChildren,
      child: CallbackShortcuts(
        bindings: {
          const SingleActivator(LogicalKeyboardKey.keyA, control: true): selectAll,
          const SingleActivator(LogicalKeyboardKey.keyA, meta: true): selectAll,
          const SingleActivator(LogicalKeyboardKey.escape): _selection.clear,
          const SingleActivator(LogicalKeyboardKey.delete): () {
            if (ItemAction.trash.isEnabledFor(selected)) {
              unawaited(FileActions(context).trash(selected));
            }
          },
        },
        child: Focus(
          autofocus: true,
          child: DragSelectArea(
            registry: _keys,
            onBackgroundTap: _selection.clear,
            onBackgroundSecondaryTap: folder.entry.permissions.canAddChildren
                ? (position) => unawaited(showNewItemMenu(context, folderId: folder.entry.id, position: position))
                : null,
            onSelect: (ids, {required additive}) =>
                _selection.replace(Selection(ids: additive ? {...selection.ids, ...ids} : ids)),
            child: switch ((entries.isEmpty, layout)) {
              (true, _) => const EmptyFolderView(),
              (false, FileLayout.list) => FileListView(
                entries: entries,
                interactions: interactions,
                scrollKey: scrollKey,
              ),
              (false, FileLayout.grid) => FileGridView(
                reservedWidth: reservedWidth,
                entries: entries,
                interactions: interactions,
                scrollKey: scrollKey,
              ),
            },
          ),
        ),
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    final folderState = ref.watch(folderControllerProvider(widget.folderId));
    final selection = ref.watch(selectionControllerProvider(widget.folderId));
    final settings = ref.watch(fileViewSettingsProvider);
    final detailsOpen = ref.watch(detailsPanelProvider);
    final sizeClass = WindowSizeClass.of(context);
    final folder = folderState.value;
    final entries = arrangeEntries(folder?.items ?? const [], settings);
    final selected = entries.where((entry) => selection.contains(entry.id)).toList();

    final body = switch (folderState) {
      AsyncValue(hasValue: true, :final value?) => _contents(
        value,
        entries,
        selection,
        reservedWidth: detailsOpen && sizeClass == WindowSizeClass.expanded ? FolderPage._panelWidth + 1 : 0,
      ),
      AsyncValue(:final error?) => FolderErrorView(
        isRoot: widget.folderId == Routes.rootFolderId,
        error: error,
        onRetry: () => ref.invalidate(folderControllerProvider(widget.folderId)),
      ),
      _ => Center(child: CircularProgressIndicator(semanticsLabel: AppLocalizations.of(context).loading)),
    };
    final page = Column(
      crossAxisAlignment: CrossAxisAlignment.stretch,
      children: [
        _header(folder, selected, selection),
        Expanded(child: body),
      ],
    );
    if (!detailsOpen || sizeClass == WindowSizeClass.compact) {
      return page;
    }

    final panel = SizedBox(
      width: FolderPage._panelWidth,
      child: DetailsPanel(
        entry: selected.length == 1 ? selected.single : (selected.isEmpty ? folder?.entry : null),
        selectedCount: selected.length,
        onClose: ref.read(detailsPanelProvider.notifier).hide,
      ),
    );
    if (sizeClass == WindowSizeClass.expanded) {
      return Row(
        crossAxisAlignment: CrossAxisAlignment.stretch,
        children: [
          Expanded(child: page),
          const VerticalDivider(width: 1),
          panel,
        ],
      );
    }
    return Stack(
      children: [
        Positioned.fill(child: page),
        PositionedDirectional(
          top: 0,
          bottom: 0,
          end: 0,
          child: Material(elevation: 4, color: Theme.of(context).colorScheme.surface, child: panel),
        ),
      ],
    );
  }
}
