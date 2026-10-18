import 'package:flutter/material.dart';

import '../../api/generated/models/item_entry.dart';
import '../../api/generated/models/item_kind.dart';
import '../../app/theme/app_tokens.dart';
import 'file_presentation.dart';
import 'item_interactions.dart';
import 'sort_menu.dart';

class FileGridView extends StatelessWidget {
  const FileGridView({super.key, required this.entries, required this.interactions});

  static const _tileExtent = 180.0;
  static const _folderExtent = 240.0;
  static const _folderHeight = 48.0;
  static const _iconSize = 48.0;
  static const _radius = 12.0;

  final List<ItemEntry> entries;
  final ItemInteractions interactions;

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
          child: CustomScrollView(
            slivers: [
              SliverPadding(
                padding: EdgeInsets.all(spacing),
                sliver: SliverGrid.builder(
                  gridDelegate: SliverGridDelegateWithMaxCrossAxisExtent(
                    maxCrossAxisExtent: _folderExtent,
                    mainAxisExtent: _folderHeight,
                    mainAxisSpacing: spacing,
                    crossAxisSpacing: spacing,
                  ),
                  itemCount: folders.length,
                  itemBuilder: (context, index) => _FolderTile(entry: folders[index], interactions: interactions),
                ),
              ),
              SliverPadding(
                padding: EdgeInsets.fromLTRB(spacing, 0, spacing, spacing),
                sliver: SliverGrid.builder(
                  gridDelegate: SliverGridDelegateWithMaxCrossAxisExtent(
                    maxCrossAxisExtent: _tileExtent,
                    mainAxisSpacing: spacing,
                    crossAxisSpacing: spacing,
                  ),
                  itemCount: files.length,
                  itemBuilder: (context, index) => _FileTile(entry: files[index], interactions: interactions),
                ),
              ),
            ],
          ),
        ),
      ],
    );
  }

  static ShapeBorder _shape(BuildContext context) => RoundedRectangleBorder(
    borderRadius: BorderRadius.circular(_radius),
    side: BorderSide(color: Theme.of(context).colorScheme.outlineVariant),
  );
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
      shape: FileGridView._shape(context),
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

class _FileTile extends StatelessWidget {
  const _FileTile({required this.entry, required this.interactions});

  final ItemEntry entry;
  final ItemInteractions interactions;

  @override
  Widget build(BuildContext context) {
    final spacing = AppTokens.of(context).spacing;
    return SelectableItem(
      entry: entry,
      interactions: interactions,
      shape: FileGridView._shape(context),
      child: Padding(
        padding: EdgeInsets.all(spacing / 2),
        child: Column(
          children: [
            Align(
              alignment: AlignmentDirectional.topEnd,
              child: ItemMoreButton(entry: entry, interactions: interactions),
            ),
            Expanded(
              child: ItemLeading(
                entry: entry,
                interactions: interactions,
                icon: iconFor(entry),
                size: FileGridView._iconSize,
              ),
            ),
            Text(entry.name, maxLines: 2, overflow: TextOverflow.ellipsis, textAlign: TextAlign.center),
          ],
        ),
      ),
    );
  }
}
