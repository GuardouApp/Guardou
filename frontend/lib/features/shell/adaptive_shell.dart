import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';

import '../../app/router/routes.dart';
import '../../app/theme/app_tokens.dart';
import '../../core/layout/window_size_class.dart';
import '../../core/status/status_bar.dart';
import '../../l10n/generated/app_localizations.dart';
import '../files/file_drop_target.dart';
import 'new_item_button.dart';
import 'shell_destination.dart';
import 'shell_navigation_drawer.dart';
import 'shell_top_bar.dart';

class AdaptiveShell extends StatelessWidget {
  const AdaptiveShell({super.key, required this.location, required this.child});

  final String location;
  final Widget child;

  @override
  Widget build(BuildContext context) {
    final selected = ShellDestination.fromPath(location);
    final currentFolderId = Routes.folderIdOf(location);
    void select(ShellDestination destination) => context.go(destination.path);

    return FileDropListener(
      child: switch (WindowSizeClass.of(context)) {
        WindowSizeClass.compact => _CompactShell(
          selected: selected,
          onSelected: select,
          currentFolderId: currentFolderId,
          child: child,
        ),
        WindowSizeClass.medium => _MediumShell(
          selected: selected,
          onSelected: select,
          currentFolderId: currentFolderId,
          child: child,
        ),
        WindowSizeClass.expanded => _ExpandedShell(
          selected: selected,
          onSelected: select,
          currentFolderId: currentFolderId,
          child: child,
        ),
      },
    );
  }
}

class _CompactShell extends StatelessWidget {
  const _CompactShell({
    required this.selected,
    required this.onSelected,
    required this.currentFolderId,
    required this.child,
  });

  final ShellDestination? selected;
  final ValueChanged<ShellDestination> onSelected;
  final String? currentFolderId;
  final Widget child;

  @override
  Widget build(BuildContext context) {
    final l10n = AppLocalizations.of(context);
    final barIndex = ShellDestination.indexIn(ShellDestination.compactBar, selected);

    return Scaffold(
      appBar: ShellTopBar(showMenuButton: true, currentFolderId: currentFolderId),
      drawer: ShellNavigationDrawer(
        selected: selected,
        onSelected: (destination) {
          Navigator.of(context).pop();
          onSelected(destination);
        },
        currentFolderId: currentFolderId,
      ),
      body: _WithStatusBar(child: child),
      floatingActionButton: NewItemButton(
        style: NewItemButtonStyle.floating,
        folderId: currentFolderId ?? Routes.rootFolderId,
      ),
      bottomNavigationBar: barIndex < 0
          ? null
          : NavigationBar(
              selectedIndex: barIndex,
              onDestinationSelected: (index) => onSelected(ShellDestination.compactBar[index]),
              destinations: [
                for (final destination in ShellDestination.compactBar)
                  NavigationDestination(
                    icon: Icon(destination.icon),
                    selectedIcon: Icon(destination.selectedIcon),
                    label: destination.shortLabel(l10n),
                  ),
              ],
            ),
    );
  }
}

class _MediumShell extends StatelessWidget {
  const _MediumShell({
    required this.selected,
    required this.onSelected,
    required this.currentFolderId,
    required this.child,
  });

  final ShellDestination? selected;
  final ValueChanged<ShellDestination> onSelected;
  final String? currentFolderId;
  final Widget child;

  @override
  Widget build(BuildContext context) {
    final l10n = AppLocalizations.of(context);
    final index = ShellDestination.indexIn(ShellDestination.navigation, selected);

    return Scaffold(
      appBar: ShellTopBar(currentFolderId: currentFolderId),
      body: Row(
        children: [
          NavigationRail(
            selectedIndex: index < 0 ? null : index,
            onDestinationSelected: (index) => onSelected(ShellDestination.navigation[index]),
            labelType: NavigationRailLabelType.all,
            backgroundColor: Colors.transparent,
            leading: NewItemButton(style: NewItemButtonStyle.small, folderId: currentFolderId ?? Routes.rootFolderId),
            destinations: [
              for (final destination in ShellDestination.navigation)
                NavigationRailDestination(
                  icon: Icon(destination.icon),
                  selectedIcon: Icon(destination.selectedIcon),
                  label: Text(destination.shortLabel(l10n)),
                ),
            ],
          ),
          Expanded(child: ContentPanel(child: child)),
        ],
      ),
    );
  }
}

class _ExpandedShell extends StatelessWidget {
  const _ExpandedShell({
    required this.selected,
    required this.onSelected,
    required this.currentFolderId,
    required this.child,
  });

  final ShellDestination? selected;
  final ValueChanged<ShellDestination> onSelected;
  final String? currentFolderId;
  final Widget child;

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: ShellTopBar(currentFolderId: currentFolderId),
      body: Row(
        children: [
          SizedBox(
            width: AppTokens.of(context).navigationWidth,
            child: ShellNavigationDrawer(
              selected: selected,
              onSelected: onSelected,
              currentFolderId: currentFolderId,
              inline: true,
            ),
          ),
          Expanded(child: ContentPanel(child: child)),
        ],
      ),
    );
  }
}

class ContentPanel extends StatelessWidget {
  const ContentPanel({super.key, required this.child});

  final Widget child;

  @override
  Widget build(BuildContext context) {
    final tokens = AppTokens.of(context);
    return Padding(
      padding: EdgeInsets.only(right: tokens.spacing, bottom: tokens.spacing),
      child: ClipRRect(
        borderRadius: BorderRadius.circular(tokens.panelRadius),
        child: ColoredBox(
          color: tokens.panelColor,
          child: _WithStatusBar(child: child),
        ),
      ),
    );
  }
}

class _WithStatusBar extends StatelessWidget {
  const _WithStatusBar({required this.child});

  final Widget child;

  @override
  Widget build(BuildContext context) {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.stretch,
      children: [
        Expanded(child: child),
        const StatusBar(),
      ],
    );
  }
}
