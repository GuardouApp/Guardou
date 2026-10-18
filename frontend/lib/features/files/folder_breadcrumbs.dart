import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';
import 'package:lucide_icons_flutter/lucide_icons.dart';

import '../../api/generated/models/breadcrumb.dart';
import '../../api/generated/models/item_details.dart';
import '../../app/router/routes.dart';
import '../../core/layout/window_size_class.dart';
import '../../l10n/generated/app_localizations.dart';
import 'file_drop_target.dart';
import 'file_presentation.dart';

String crumbRoute(Breadcrumb crumb) => crumb.name.isEmpty ? Routes.files : Routes.folder(crumb.id);

class FolderBreadcrumbs extends StatelessWidget {
  const FolderBreadcrumbs({super.key, required this.folder});

  static const _compactVisibleAncestors = 1;

  final ItemDetails folder;

  @override
  Widget build(BuildContext context) {
    final compact = WindowSizeClass.of(context) == WindowSizeClass.compact;
    final ancestors = folder.ancestors;
    final collapsed = compact && ancestors.length > _compactVisibleAncestors + 1
        ? ancestors.sublist(_compactVisibleAncestors)
        : const <Breadcrumb>[];
    final visible = ancestors.take(ancestors.length - collapsed.length);

    return Align(
      alignment: AlignmentDirectional.centerStart,
      child: SingleChildScrollView(
        scrollDirection: Axis.horizontal,
        reverse: true,
        child: Row(
          children: [
            for (final ancestor in visible) ...[_Crumb(crumb: ancestor), const _Separator()],
            if (collapsed.isNotEmpty) ...[_CollapsedCrumbs(crumbs: collapsed), const _Separator()],
            Text(folderLabel(folder.entry, AppLocalizations.of(context))),
          ],
        ),
      ),
    );
  }
}

class _Crumb extends StatelessWidget {
  const _Crumb({required this.crumb});

  final Breadcrumb crumb;

  @override
  Widget build(BuildContext context) {
    return FileDropTarget(
      folderId: crumb.id,
      enabled: true,
      child: InkWell(
        onTap: () => context.go(crumbRoute(crumb)),
        borderRadius: BorderRadius.circular(8),
        child: Text(
          breadcrumbLabel(crumb, AppLocalizations.of(context)),
          style: TextStyle(color: Theme.of(context).colorScheme.onSurfaceVariant),
        ),
      ),
    );
  }
}

class _CollapsedCrumbs extends StatelessWidget {
  const _CollapsedCrumbs({required this.crumbs});

  final List<Breadcrumb> crumbs;

  @override
  Widget build(BuildContext context) {
    return MenuAnchor(
      builder: (context, menu, _) => IconButton(
        tooltip: AppLocalizations.of(context).moreFolders,
        onPressed: () => menu.isOpen ? menu.close() : menu.open(),
        icon: const Icon(LucideIcons.ellipsis),
      ),
      menuChildren: [
        for (final crumb in crumbs)
          MenuItemButton(
            leadingIcon: const Icon(LucideIcons.folder),
            onPressed: () => context.go(crumbRoute(crumb)),
            child: Text(breadcrumbLabel(crumb, AppLocalizations.of(context))),
          ),
      ],
    );
  }
}

class _Separator extends StatelessWidget {
  const _Separator();

  @override
  Widget build(BuildContext context) {
    return Padding(
      padding: const EdgeInsets.symmetric(horizontal: 8),
      child: Icon(LucideIcons.chevronRight, size: 18, color: Theme.of(context).colorScheme.onSurfaceVariant),
    );
  }
}
