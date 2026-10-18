import 'package:flutter/widgets.dart';
import 'package:lucide_icons_flutter/lucide_icons.dart';
import 'package:lucide_icons_flutter/lucide_icons_600.dart';

import '../../app/router/routes.dart';
import '../../l10n/generated/app_localizations.dart';

enum AdminSection {
  users(Routes.adminUsers, LucideIcons.users, LucideIcons600.users),
  tasks(Routes.adminTasks, LucideIcons.listTodo, LucideIcons600.listTodo),
  settings(Routes.adminSettings, LucideIcons.settings, LucideIcons600.settings);

  const AdminSection(this.path, this.icon, this.selectedIcon);

  final String path;
  final IconData icon;
  final IconData selectedIcon;

  String label(AppLocalizations l10n) => switch (this) {
    users => l10n.users,
    tasks => l10n.tasks,
    settings => l10n.settings,
  };

  static AdminSection? fromPath(String path) {
    for (final section in values) {
      if (path == section.path || path.startsWith('${section.path}/')) {
        return section;
      }
    }
    return null;
  }
}
