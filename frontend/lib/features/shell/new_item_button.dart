import 'dart:async';

import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:lucide_icons_flutter/lucide_icons_600.dart';

import '../../l10n/generated/app_localizations.dart';
import '../files/file_actions.dart';
import '../files/folder_controller.dart';
import '../files/new_item_menu.dart';

enum NewItemButtonStyle { floating, small, extended }

class NewItemButton extends ConsumerWidget {
  const NewItemButton({super.key, required this.style, required this.folderId});

  final NewItemButtonStyle style;
  final String folderId;

  void _create(BuildContext context, NewItemKind kind) => unawaited(kind.create(FileActions(context), folderId));

  Future<void> _showSheet(BuildContext context) async {
    final l10n = AppLocalizations.of(context);
    final kind = await showModalBottomSheet<NewItemKind>(
      context: context,
      showDragHandle: true,
      builder: (context) => SafeArea(
        child: ListView(
          shrinkWrap: true,
          children: [
            for (final kind in NewItemKind.values)
              ListTile(
                leading: Icon(kind.icon),
                title: Text(kind.label(l10n)),
                onTap: () => Navigator.of(context).pop(kind),
              ),
          ],
        ),
      ),
    );
    if (kind != null && context.mounted) {
      _create(context, kind);
    }
  }

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final l10n = AppLocalizations.of(context);
    final canAdd = ref.watch(
      folderControllerProvider(folderId).select((folder) => folder.value?.entry.permissions.canAddChildren ?? false),
    );
    const icon = Icon(LucideIcons600.plus);

    if (style == NewItemButtonStyle.floating) {
      return FloatingActionButton(
        heroTag: null,
        tooltip: l10n.newItem,
        onPressed: canAdd ? () => unawaited(_showSheet(context)) : null,
        child: icon,
      );
    }
    return MenuAnchor(
      menuChildren: [
        for (final kind in NewItemKind.values)
          MenuItemButton(
            leadingIcon: Icon(kind.icon, size: 18),
            onPressed: () => _create(context, kind),
            child: Text(kind.label(l10n)),
          ),
      ],
      builder: (context, menu, _) {
        final VoidCallback? toggle = canAdd ? () => menu.isOpen ? menu.close() : menu.open() : null;
        return switch (style) {
          NewItemButtonStyle.extended => FloatingActionButton.extended(
            heroTag: null,
            elevation: 1,
            onPressed: toggle,
            icon: icon,
            label: Text(l10n.newItem),
          ),
          _ => FloatingActionButton(heroTag: null, tooltip: l10n.newItem, elevation: 0, onPressed: toggle, child: icon),
        };
      },
    );
  }
}
