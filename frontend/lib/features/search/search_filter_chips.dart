import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:lucide_icons_flutter/lucide_icons.dart';

import '../../l10n/generated/app_localizations.dart';
import 'search_controller.dart';
import 'search_filters.dart';

class SearchFilterChips extends ConsumerWidget {
  const SearchFilterChips({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final l10n = AppLocalizations.of(context);
    final search = ref.watch(itemSearchControllerProvider);
    final controller = ref.read(itemSearchControllerProvider.notifier);
    final filters = search.filters;
    final now = DateTime.now();

    return Wrap(
      spacing: 8,
      runSpacing: 8,
      children: [
        if (search.folderId != null)
          FilterChip(
            label: Text(l10n.searchFolderOnly),
            selected: search.folderOnly,
            onSelected: controller.setFolderOnly,
          ),
        _MenuChip(
          label: filters.types.isEmpty
              ? l10n.searchType
              : filters.types.map((type) => searchTypeLabel(type, l10n)).join(', '),
          selected: filters.types.isNotEmpty,
          items: [
            for (final type in SearchType.values)
              CheckboxMenuButton(
                value: filters.types.contains(type),
                closeOnActivate: false,
                onChanged: (_) => controller.toggleType(type),
                child: Text(searchTypeLabel(type, l10n)),
              ),
          ],
        ),
        _MenuChip(
          label: switch (filters.modified) {
            final modified? => searchModifiedLabel(modified, now, l10n),
            null => l10n.searchModified,
          },
          selected: filters.modified != null,
          items: [
            MenuItemButton(onPressed: () => controller.setModified(null), child: Text(l10n.searchAny)),
            for (final modified in SearchModified.values)
              MenuItemButton(
                onPressed: () => controller.setModified(modified),
                child: Text(searchModifiedLabel(modified, now, l10n)),
              ),
          ],
        ),
        _MenuChip(
          label: switch (filters.size) {
            final size? => searchSizeLabel(size, l10n),
            null => l10n.searchSize,
          },
          selected: filters.size != null,
          items: [
            MenuItemButton(onPressed: () => controller.setSize(null), child: Text(l10n.searchAny)),
            for (final size in SearchSize.values)
              MenuItemButton(onPressed: () => controller.setSize(size), child: Text(searchSizeLabel(size, l10n))),
          ],
        ),
      ],
    );
  }
}

class _MenuChip extends StatelessWidget {
  const _MenuChip({required this.label, required this.selected, required this.items});

  final String label;
  final bool selected;
  final List<Widget> items;

  @override
  Widget build(BuildContext context) => MenuAnchor(
    menuChildren: items,
    builder: (context, menu, _) => FilterChip(
      label: Row(
        mainAxisSize: MainAxisSize.min,
        children: [
          Flexible(child: Text(label, overflow: TextOverflow.ellipsis)),
          const SizedBox(width: 4),
          const Icon(LucideIcons.chevronDown, size: 16),
        ],
      ),
      selected: selected,
      onSelected: (_) => menu.isOpen ? menu.close() : menu.open(),
    ),
  );
}

String searchTypeLabel(SearchType type, AppLocalizations l10n) => switch (type) {
  SearchType.folder => l10n.searchTypeFolder,
  SearchType.image => l10n.searchTypeImage,
  SearchType.video => l10n.searchTypeVideo,
  SearchType.audio => l10n.searchTypeAudio,
  SearchType.pdf => l10n.searchTypePdf,
  SearchType.document => l10n.searchTypeDocument,
  SearchType.spreadsheet => l10n.searchTypeSpreadsheet,
  SearchType.presentation => l10n.searchTypePresentation,
  SearchType.archive => l10n.searchTypeArchive,
};

String searchModifiedLabel(SearchModified modified, DateTime now, AppLocalizations l10n) => switch (modified) {
  SearchModified.today => l10n.searchModifiedToday,
  SearchModified.last7Days => l10n.searchModifiedLast7Days,
  SearchModified.last30Days => l10n.searchModifiedLast30Days,
  SearchModified.thisYear => l10n.searchModifiedThisYear('${now.year}'),
  SearchModified.lastYear => l10n.searchModifiedLastYear('${now.year - 1}'),
};

String searchSizeLabel(SearchSize size, AppLocalizations l10n) => switch (size) {
  SearchSize.smallerThan1Mb => l10n.searchSizeSmallerThan1Mb,
  SearchSize.largerThan1Mb => l10n.searchSizeLargerThan1Mb,
  SearchSize.largerThan100Mb => l10n.searchSizeLargerThan100Mb,
  SearchSize.largerThan1Gb => l10n.searchSizeLargerThan1Gb,
};
