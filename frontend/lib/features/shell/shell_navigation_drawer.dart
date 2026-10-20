import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:lucide_icons_flutter/lucide_icons.dart';

import '../../app/router/routes.dart';
import '../../app/theme/app_tokens.dart';
import '../../l10n/generated/app_localizations.dart';
import '../files/folder_tree.dart';
import '../files/folder_tree_controller.dart';
import '../files/storage_usage_indicator.dart';
import 'new_item_button.dart';
import 'shell_destination.dart';

class ShellNavigationDrawer extends ConsumerWidget {
  static const _destinationDecorationWidth = 96.0;

  const ShellNavigationDrawer({
    super.key,
    required this.selected,
    required this.onSelected,
    required this.currentFolderId,
    required this.onNavigate,
    this.inline = false,
  });

  final ShellDestination? selected;
  final ValueChanged<ShellDestination> onSelected;
  final String? currentFolderId;
  final ValueChanged<String> onNavigate;
  final bool inline;

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final l10n = AppLocalizations.of(context);
    final tokens = AppTokens.of(context);
    final index = ShellDestination.indexIn(ShellDestination.navigation, selected);
    final treeExpanded = ref.watch(folderTreeExpandedProvider);

    return NavigationDrawer(
      selectedIndex: index < 0 ? null : index,
      onDestinationSelected: (index) => onSelected(ShellDestination.navigation[index]),
      backgroundColor: inline ? Colors.transparent : null,
      elevation: inline ? 0 : null,
      children: [
        Padding(
          padding: EdgeInsets.fromLTRB(tokens.spacing, tokens.spacing / 2, tokens.spacing, tokens.spacing),
          child: Align(
            alignment: AlignmentDirectional.centerStart,
            child: NewItemButton(
              style: NewItemButtonStyle.extended,
              folderId: currentFolderId ?? Routes.rootFolderId,
            ),
          ),
        ),
        for (final destination in ShellDestination.navigation) ...[
          NavigationDrawerDestination(
            icon: Icon(destination.icon),
            selectedIcon: Icon(destination.selectedIcon),
            label: SizedBox(
              width: tokens.navigationWidth - _destinationDecorationWidth,
              child: Row(
                children: [
                  Expanded(child: Text(destination.label(l10n), maxLines: 1, overflow: TextOverflow.ellipsis)),
                  if (destination == ShellDestination.files)
                    IconButton(
                      tooltip: treeExpanded ? l10n.collapseFolder : l10n.expandFolder,
                      visualDensity: VisualDensity.compact,
                      iconSize: 18,
                      onPressed: ref.read(folderTreeExpandedProvider.notifier).toggle,
                      icon: Icon(treeExpanded ? LucideIcons.chevronDown : LucideIcons.chevronRight),
                    ),
                ],
              ),
            ),
          ),
          if (destination == ShellDestination.files && treeExpanded)
            Padding(
              padding: EdgeInsets.symmetric(horizontal: tokens.spacing * 0.75),
              child: FolderTree(currentFolderId: currentFolderId, onNavigate: onNavigate),
            ),
        ],
        const StorageUsageIndicator(),
      ],
    );
  }
}
