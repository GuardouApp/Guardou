import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:lucide_icons_flutter/lucide_icons.dart';

import '../../api/generated/models/item_details.dart';
import '../../core/layout/window_size_class.dart';
import '../../core/widgets/page_header.dart';
import '../../l10n/generated/app_localizations.dart';
import 'details_panel_controller.dart';
import 'file_view_settings.dart';
import 'folder_breadcrumbs.dart';

class FolderHeader extends ConsumerWidget {
  const FolderHeader({super.key, required this.folder});

  final ItemDetails? folder;

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final l10n = AppLocalizations.of(context);
    final compact = WindowSizeClass.of(context) == WindowSizeClass.compact;
    final current = folder;
    final readOnly = current != null && !current.entry.permissions.canAddChildren;

    return PageHeader(
      child: Row(
        children: [
          Expanded(child: current == null ? const SizedBox.shrink() : FolderBreadcrumbs(folder: current)),
          if (readOnly)
            Tooltip(
              message: l10n.readOnlyFolder,
              child: Icon(LucideIcons.lock, size: 18, color: Theme.of(context).colorScheme.onSurfaceVariant),
            ),
          const _LayoutToggle(),
          if (!compact)
            IconButton(
              tooltip: l10n.actionDetails,
              isSelected: ref.watch(detailsPanelProvider),
              onPressed: ref.read(detailsPanelProvider.notifier).toggle,
              icon: const Icon(LucideIcons.info),
            ),
        ],
      ),
    );
  }
}

class _LayoutToggle extends ConsumerWidget {
  const _LayoutToggle();

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final l10n = AppLocalizations.of(context);
    final layout = ref.watch(fileViewSettingsProvider.select((settings) => settings.layout));
    return IconButton(
      tooltip: layout == FileLayout.list ? l10n.viewAsGrid : l10n.viewAsList,
      onPressed: ref.read(fileViewSettingsProvider.notifier).toggleLayout,
      icon: Icon(layout == FileLayout.list ? LucideIcons.layoutGrid : LucideIcons.layoutList),
    );
  }
}
