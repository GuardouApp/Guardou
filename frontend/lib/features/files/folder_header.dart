import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:lucide_icons_flutter/lucide_icons.dart';

import '../../api/generated/models/item_details.dart';
import '../../core/layout/window_size_class.dart';
import '../../core/widgets/page_header.dart';
import '../../l10n/generated/app_localizations.dart';
import 'details_panel_controller.dart';
import 'folder_breadcrumbs.dart';
import 'folder_layouts.dart';

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
          if (current != null) _LayoutToggle(folderId: current.entry.id),
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
  const _LayoutToggle({required this.folderId});

  final String folderId;

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final l10n = AppLocalizations.of(context);
    final layout = ref.watch(folderLayoutsProvider.select((layouts) => layouts.of(folderId)));
    return IconButton(
      tooltip: layout == FileLayout.list ? l10n.viewAsGrid : l10n.viewAsList,
      onPressed: () => ref.read(folderLayoutsProvider.notifier).toggle(folderId),
      icon: Icon(layout == FileLayout.list ? LucideIcons.layoutGrid : LucideIcons.layoutList),
    );
  }
}
