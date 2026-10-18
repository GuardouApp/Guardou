import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:lucide_icons_flutter/lucide_icons.dart';

import '../../l10n/generated/app_localizations.dart';
import 'file_view_settings.dart';

String sortFieldLabel(SortField field, AppLocalizations l10n) => switch (field) {
  SortField.name => l10n.columnName,
  SortField.modified => l10n.columnModified,
  SortField.created => l10n.columnCreated,
  SortField.size => l10n.columnSize,
  SortField.kind => l10n.columnKind,
};

IconData sortDirectionIcon({required bool ascending}) => ascending ? LucideIcons.arrowUp : LucideIcons.arrowDown;

class SortMenu extends ConsumerWidget {
  const SortMenu({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final l10n = AppLocalizations.of(context);
    final settings = ref.watch(fileViewSettingsProvider);
    final controller = ref.read(fileViewSettingsProvider.notifier);

    return Align(
      alignment: AlignmentDirectional.centerStart,
      child: MenuAnchor(
        builder: (context, menu, _) => TextButton.icon(
          onPressed: () => menu.isOpen ? menu.close() : menu.open(),
          icon: Icon(sortDirectionIcon(ascending: settings.ascending), size: 16),
          label: Text(sortFieldLabel(settings.sortField, l10n)),
        ),
        menuChildren: [
          for (final field in SortField.values)
            MenuItemButton(
              leadingIcon: field == settings.sortField
                  ? Icon(sortDirectionIcon(ascending: settings.ascending), size: 16)
                  : const SizedBox(width: 16),
              onPressed: () => controller.sortBy(field),
              child: Text(sortFieldLabel(field, l10n)),
            ),
        ],
      ),
    );
  }
}
