import 'dart:math';

import 'package:clock/clock.dart';
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../api/generated/models/item_entry.dart';
import '../../api/generated/models/item_kind.dart';
import '../../app/theme/app_tokens.dart';
import '../../core/widgets/user_avatar.dart';
import '../../l10n/generated/app_localizations.dart';
import '../auth/session_controller.dart';
import 'file_presentation.dart';
import 'item_interactions.dart';
import 'item_thumbnail.dart';
import 'remembered_scroll.dart';
import 'sort_menu.dart';

class FileGridView extends StatelessWidget {
  const FileGridView({super.key, required this.entries, required this.interactions, required this.scrollKey});

  static const _minimumTileWidth = 250.0;
  static const _folderHeight = 48.0;
  static const _radius = 12.0;

  final List<ItemEntry> entries;
  final ItemInteractions interactions;
  final String scrollKey;

  @override
  Widget build(BuildContext context) {
    final spacing = AppTokens.of(context).spacing;
    final folders = entries.where((entry) => entry.kind == ItemKind.folder).toList();
    final files = entries.where((entry) => entry.kind != ItemKind.folder).toList();

    return Column(
      crossAxisAlignment: CrossAxisAlignment.stretch,
      children: [
        const SortMenu(),
        Expanded(
          child: RememberedScroll(
            storageKey: scrollKey,
            builder: (context, controller) => CustomScrollView(
              controller: controller,
              slivers: [
                SliverPadding(
                  padding: EdgeInsets.all(spacing),
                  sliver: SliverLayoutBuilder(
                    builder: (context, constraints) => SliverGrid.builder(
                      gridDelegate: SliverGridDelegateWithFixedCrossAxisCount(
                        crossAxisCount: _columnsFor(constraints.crossAxisExtent, spacing),
                        mainAxisExtent: _folderHeight,
                        mainAxisSpacing: spacing,
                        crossAxisSpacing: spacing,
                      ),
                      itemCount: folders.length,
                      itemBuilder: (context, index) => _FolderTile(entry: folders[index], interactions: interactions),
                    ),
                  ),
                ),
                SliverPadding(
                  padding: EdgeInsets.fromLTRB(spacing, 0, spacing, spacing),
                  sliver: SliverLayoutBuilder(
                    builder: (context, constraints) {
                      final width = constraints.crossAxisExtent;
                      final columns = _columnsFor(width, spacing);
                      final tileWidth = (width - spacing * (columns - 1)) / columns;
                      return SliverGrid.builder(
                        gridDelegate: SliverGridDelegateWithFixedCrossAxisCount(
                          crossAxisCount: columns,
                          mainAxisExtent: _FileCard.heightFor(tileWidth, spacing),
                          mainAxisSpacing: spacing,
                          crossAxisSpacing: spacing,
                        ),
                        itemCount: files.length,
                        itemBuilder: (context, index) => _FileCard(entry: files[index], interactions: interactions),
                      );
                    },
                  ),
                ),
              ],
            ),
          ),
        ),
      ],
    );
  }

  static int _columnsFor(double width, double spacing) =>
      max(1, ((width + spacing) / (_minimumTileWidth + spacing)).floor());

  static ShapeBorder _shape() => RoundedRectangleBorder(borderRadius: BorderRadius.circular(_radius));

  static Color _cardColor(BuildContext context) => Theme.of(context).colorScheme.surfaceContainerLow;
}

class _FolderTile extends StatelessWidget {
  const _FolderTile({required this.entry, required this.interactions});

  final ItemEntry entry;
  final ItemInteractions interactions;

  @override
  Widget build(BuildContext context) {
    final spacing = AppTokens.of(context).spacing;
    return SelectableItem(
      entry: entry,
      interactions: interactions,
      shape: FileGridView._shape(),
      color: FileGridView._cardColor(context),
      child: Padding(
        padding: EdgeInsetsDirectional.only(start: spacing),
        child: Row(
          children: [
            ItemLeading(entry: entry, interactions: interactions, icon: iconFor(entry)),
            SizedBox(width: spacing),
            Expanded(child: Text(entry.name, maxLines: 1, overflow: TextOverflow.ellipsis)),
            ItemMoreButton(entry: entry, interactions: interactions),
          ],
        ),
      ),
    );
  }
}

class _FileCard extends ConsumerWidget {
  const _FileCard({required this.entry, required this.interactions});

  static const _headerHeight = 52.0;
  static const _footerHeight = 28.0;
  static const _previewAspectRatio = 4 / 3;
  static const _iconSize = 20.0;
  static const _placeholderIconSize = 56.0;
  static const _previewRadius = 8.0;
  static const _avatarRadius = 12.0;

  final ItemEntry entry;
  final ItemInteractions interactions;

  static double heightFor(double width, double spacing) {
    final insets = _insets(spacing);
    final previewHeight = (width - insets.horizontal) / _previewAspectRatio;
    return _headerHeight + previewHeight + _gap(spacing) + _footerHeight + insets.bottom;
  }

  static EdgeInsets _insets(double spacing) => EdgeInsets.fromLTRB(spacing * 0.75, 0, spacing * 0.75, spacing * 0.75);

  static double _gap(double spacing) => spacing / 2;

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final l10n = AppLocalizations.of(context);
    final theme = Theme.of(context);
    final spacing = AppTokens.of(context).spacing;
    final username = ref.watch(sessionControllerProvider).value?.username ?? '';
    final placeholder = Center(
      child: Icon(iconFor(entry), size: _placeholderIconSize, color: theme.colorScheme.onSurfaceVariant),
    );

    return SelectableItem(
      entry: entry,
      interactions: interactions,
      shape: FileGridView._shape(),
      color: FileGridView._cardColor(context),
      child: Padding(
        padding: _insets(spacing),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.stretch,
          children: [
            SizedBox(
              height: _headerHeight,
              child: Row(
                children: [
                  ItemLeading(
                    entry: entry,
                    interactions: interactions,
                    icon: iconFor(entry),
                    size: _iconSize,
                    showPreview: false,
                  ),
                  SizedBox(width: spacing * 0.75),
                  Expanded(
                    child: Text(
                      entry.name,
                      maxLines: 1,
                      overflow: TextOverflow.ellipsis,
                      style: theme.textTheme.titleSmall,
                    ),
                  ),
                  ItemMoreButton(entry: entry, interactions: interactions),
                ],
              ),
            ),
            AspectRatio(
              aspectRatio: _previewAspectRatio,
              child: ClipRRect(
                borderRadius: BorderRadius.circular(_previewRadius),
                child: ColoredBox(
                  color: theme.colorScheme.surface,
                  child: entry.hasPreview
                      ? ItemThumbnail(key: ValueKey(entry.id), entry: entry, placeholder: placeholder)
                      : placeholder,
                ),
              ),
            ),
            SizedBox(height: _gap(spacing)),
            SizedBox(
              height: _footerHeight,
              child: Row(
                children: [
                  UserAvatar(username: username, radius: _avatarRadius),
                  SizedBox(width: spacing / 2),
                  Expanded(
                    child: Text(
                      l10n.fileUploadedByYou(formatCardDate(entry.created, clock.now(), l10n.localeName)),
                      maxLines: 1,
                      overflow: TextOverflow.ellipsis,
                      style: theme.textTheme.bodySmall?.copyWith(color: theme.colorScheme.onSurfaceVariant),
                    ),
                  ),
                ],
              ),
            ),
          ],
        ),
      ),
    );
  }
}
