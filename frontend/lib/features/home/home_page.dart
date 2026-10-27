import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:lucide_icons_flutter/lucide_icons.dart';

import '../../app/router/routes.dart';
import '../../l10n/generated/app_localizations.dart';
import '../files/folder_states.dart';
import '../files/item_collection_page.dart';
import 'recent_items.dart';

class HomePage extends ConsumerWidget {
  const HomePage({super.key});

  static const layoutKey = Routes.home;

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final l10n = AppLocalizations.of(context);
    return ItemCollectionPage(
      title: l10n.navigationHome,
      layoutKey: layoutKey,
      items: ref.watch(recentItemsProvider),
      arrange: (items) => items,
      onRetry: () => ref.invalidate(recentItemsProvider),
      empty: FolderMessage(
        icon: LucideIcons.clock,
        title: l10n.recentEmptyTitle,
        message: l10n.recentEmptyMessage,
      ),
    );
  }
}
