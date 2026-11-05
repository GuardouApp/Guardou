import 'package:flutter_riverpod/misc.dart';

import '../trash/trash_controller.dart';
import 'folder_controller.dart';
import 'recent_items.dart';
import 'starred_items.dart';
import 'storage_usage_controller.dart';

void refreshFileViews(void Function(ProviderOrFamily provider) invalidate) {
  for (final provider in <ProviderOrFamily>[
    folderControllerProvider,
    childrenControllerProvider,
    recentItemsProvider,
    starredItemsProvider,
    storageUsageControllerProvider,
    trashControllerProvider,
  ]) {
    invalidate(provider);
  }
}
