import 'package:flutter/widgets.dart';
import 'package:lucide_icons_flutter/lucide_icons.dart';
import 'package:lucide_icons_flutter/lucide_icons_600.dart';

import '../../app/router/routes.dart';
import '../../l10n/generated/app_localizations.dart';

enum ShellDestination {
  home(Routes.home, LucideIcons.house, LucideIcons600.house),
  files(Routes.files, LucideIcons.folder, LucideIcons600.folder),
  starred(Routes.starred, LucideIcons.star, LucideIcons600.star),
  shared(Routes.shared, LucideIcons.users, LucideIcons600.users),
  trash(Routes.trash, LucideIcons.trash, LucideIcons600.trash);

  const ShellDestination(this.path, this.icon, this.selectedIcon);

  static const compactBar = [home, starred, shared, files];
  static const navigation = [home, files, starred, shared, trash];

  final String path;
  final IconData icon;
  final IconData selectedIcon;

  String label(AppLocalizations l10n) => switch (this) {
    home => l10n.navigationHome,
    files => l10n.navigationFiles,
    starred => l10n.navigationStarred,
    shared => l10n.navigationSharedWithMe,
    trash => l10n.navigationTrash,
  };

  String shortLabel(AppLocalizations l10n) => this == shared ? l10n.navigationShared : label(l10n);

  static int indexIn(List<ShellDestination> destinations, ShellDestination? destination) =>
      destination == null ? -1 : destinations.indexOf(destination);

  List<String> get _paths => this == files ? const [Routes.files, Routes.folders, Routes.file] : [path];

  static ShellDestination? fromPath(String path) {
    for (final destination in values) {
      if (destination._paths.any((prefix) => path == prefix || path.startsWith('$prefix/'))) {
        return destination;
      }
    }
    return null;
  }
}
