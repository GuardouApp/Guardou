import 'package:flutter/foundation.dart';
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:lucide_icons_flutter/lucide_icons.dart';

import '../../api/generated/models/item_entry.dart';
import '../../api/generated/models/item_kind.dart';
import '../../app/router/routes.dart';
import '../../l10n/generated/app_localizations.dart';
import 'file_arrangement.dart';
import 'file_drop_target.dart';
import 'file_presentation.dart';
import 'file_view_settings.dart';
import 'folder_controller.dart';

@immutable
class FolderTreeFocus {
  const FolderTreeFocus({this.currentId, this.ancestorIds = const {}});

  final String? currentId;
  final Set<String> ancestorIds;

  @override
  bool operator ==(Object other) =>
      other is FolderTreeFocus && other.currentId == currentId && setEquals(other.ancestorIds, ancestorIds);

  @override
  int get hashCode => Object.hash(currentId, Object.hashAllUnordered(ancestorIds));
}

class FolderTree extends ConsumerWidget {
  const FolderTree({super.key, required this.currentFolderId, required this.onNavigate});

  final String? currentFolderId;
  final ValueChanged<String> onNavigate;

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final currentId = currentFolderId;
    final focus = currentId == null
        ? const FolderTreeFocus()
        : ref.watch(
            folderControllerProvider(currentId).select(
              (folder) => FolderTreeFocus(
                currentId: folder.value?.entry.id,
                ancestorIds: {...?folder.value?.ancestors.map((ancestor) => ancestor.id)},
              ),
            ),
          );
    return FolderTreeChildren(folderId: Routes.rootFolderId, focus: focus, onNavigate: onNavigate, depth: 1);
  }
}

class FolderTreeChildren extends ConsumerWidget {
  const FolderTreeChildren({
    super.key,
    required this.folderId,
    required this.focus,
    required this.onNavigate,
    required this.depth,
  });

  final String folderId;
  final FolderTreeFocus focus;
  final ValueChanged<String> onNavigate;
  final int depth;

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final children = arrangeEntries([...?ref.watch(childrenControllerProvider(folderId)).value], const FileViewSettings());

    return Column(
      crossAxisAlignment: CrossAxisAlignment.stretch,
      children: [
        for (final child in children)
          if (child.kind == ItemKind.folder)
            _FolderTreeNode(key: ValueKey(child.id), folder: child, focus: focus, onNavigate: onNavigate, depth: depth)
          else
            _TreeTile(
              key: ValueKey(child.id),
              icon: iconFor(child),
              label: child.name,
              depth: depth,
              selected: false,
              onTap: () => onNavigate(Routes.fileItem(child.id)),
            ),
      ],
    );
  }
}

class _FolderTreeNode extends StatefulWidget {
  const _FolderTreeNode({
    super.key,
    required this.folder,
    required this.focus,
    required this.onNavigate,
    required this.depth,
  });

  final ItemEntry folder;
  final FolderTreeFocus focus;
  final ValueChanged<String> onNavigate;
  final int depth;

  @override
  State<_FolderTreeNode> createState() => _FolderTreeNodeState();
}

class _FolderTreeNodeState extends State<_FolderTreeNode> {
  late bool _expanded = _containsCurrent;

  bool get _containsCurrent => widget.focus.ancestorIds.contains(widget.folder.id);

  @override
  void didUpdateWidget(_FolderTreeNode oldWidget) {
    super.didUpdateWidget(oldWidget);
    if (widget.focus != oldWidget.focus && _containsCurrent) {
      _expanded = true;
    }
  }

  @override
  Widget build(BuildContext context) {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.stretch,
      children: [
        FileDropTarget(
          folderId: widget.folder.id,
          enabled: widget.folder.permissions.canAddChildren,
          child: _TreeTile(
            icon: LucideIcons.folder,
            label: widget.folder.name,
            depth: widget.depth,
            selected: widget.folder.id == widget.focus.currentId,
            expanded: _expanded,
            onToggle: () => setState(() => _expanded = !_expanded),
            onTap: () => widget.onNavigate(Routes.folder(widget.folder.id)),
          ),
        ),
        if (_expanded)
          FolderTreeChildren(
            folderId: widget.folder.id,
            focus: widget.focus,
            onNavigate: widget.onNavigate,
            depth: widget.depth + 1,
          ),
      ],
    );
  }
}

class _TreeTile extends StatelessWidget {
  const _TreeTile({
    super.key,
    required this.icon,
    required this.label,
    required this.depth,
    required this.selected,
    required this.onTap,
    this.expanded = false,
    this.onToggle,
  });

  static const _height = 36.0;
  static const _indent = 12.0;
  static const _radius = 18.0;
  static const _toggleWidth = 40.0;

  final IconData icon;
  final String label;
  final int depth;
  final bool selected;
  final VoidCallback onTap;
  final bool expanded;
  final VoidCallback? onToggle;

  @override
  Widget build(BuildContext context) {
    final l10n = AppLocalizations.of(context);
    final colorScheme = Theme.of(context).colorScheme;
    final foreground = selected ? colorScheme.onSecondaryContainer : colorScheme.onSurfaceVariant;

    return Material(
      color: selected ? colorScheme.secondaryContainer : Colors.transparent,
      shape: const StadiumBorder(),
      clipBehavior: Clip.antiAlias,
      child: InkWell(
        onTap: onTap,
        borderRadius: BorderRadius.circular(_radius),
        child: SizedBox(
          height: _height,
          child: Row(
            children: [
              SizedBox(width: depth * _indent),
              if (onToggle case final toggle?)
                IconButton(
                  tooltip: expanded ? l10n.collapseFolder : l10n.expandFolder,
                  visualDensity: VisualDensity.compact,
                  iconSize: 16,
                  color: foreground,
                  onPressed: toggle,
                  icon: Icon(expanded ? LucideIcons.chevronDown : LucideIcons.chevronRight),
                )
              else
                const SizedBox(width: _toggleWidth),
              Icon(icon, size: 18, color: foreground),
              const SizedBox(width: 8),
              Expanded(
                child: Text(
                  label,
                  maxLines: 1,
                  overflow: TextOverflow.ellipsis,
                  style: TextStyle(color: foreground),
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }
}
