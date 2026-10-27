import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:lucide_icons_flutter/lucide_icons.dart';

import '../../app/router/routes.dart';
import '../../l10n/generated/app_localizations.dart';
import '../files/file_arrangement.dart';
import '../files/file_view_settings.dart';
import '../files/folder_states.dart';
import '../files/item_collection_page.dart';
import 'shared_with_me.dart';

class SharedWithMePage extends ConsumerWidget {
  const SharedWithMePage({super.key});

  static const layoutKey = Routes.shared;

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final l10n = AppLocalizations.of(context);
    final settings = ref.watch(fileViewSettingsProvider);
    return ItemCollectionPage(
      title: l10n.navigationSharedWithMe,
      layoutKey: layoutKey,
      items: ref.watch(sharedWithMeProvider),
      arrange: (items) => arrangeEntries(items, settings),
      onRetry: () => ref.invalidate(sharedWithMeProvider),
      empty: FolderMessage(
        icon: LucideIcons.users,
        title: l10n.sharedWithMeEmptyTitle,
        message: l10n.sharedWithMeEmptyMessage,
      ),
    );
  }
}
