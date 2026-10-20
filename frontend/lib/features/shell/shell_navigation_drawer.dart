import 'package:flutter/material.dart';

import '../../app/router/routes.dart';
import '../../app/theme/app_tokens.dart';
import '../../l10n/generated/app_localizations.dart';
import '../files/storage_usage_indicator.dart';
import 'new_item_button.dart';
import 'shell_destination.dart';

class ShellNavigationDrawer extends StatelessWidget {
  static const _destinationDecorationWidth = 96.0;

  const ShellNavigationDrawer({
    super.key,
    required this.selected,
    required this.onSelected,
    required this.currentFolderId,
    this.inline = false,
  });

  final ShellDestination? selected;
  final ValueChanged<ShellDestination> onSelected;
  final String? currentFolderId;
  final bool inline;

  @override
  Widget build(BuildContext context) {
    final l10n = AppLocalizations.of(context);
    final tokens = AppTokens.of(context);
    final index = ShellDestination.indexIn(ShellDestination.navigation, selected);

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
        for (final destination in ShellDestination.navigation)
          NavigationDrawerDestination(
            icon: Icon(destination.icon),
            selectedIcon: Icon(destination.selectedIcon),
            label: ConstrainedBox(
              constraints: BoxConstraints(maxWidth: tokens.navigationWidth - _destinationDecorationWidth),
              child: Text(destination.label(l10n), maxLines: 1, overflow: TextOverflow.ellipsis),
            ),
          ),
        const StorageUsageIndicator(),
      ],
    );
  }
}
