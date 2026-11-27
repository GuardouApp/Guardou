import 'package:flutter_riverpod/misc.dart';

import '../shares/shared_with_me.dart';
import '../trash/trash_controller.dart';
import 'folder_controller.dart';
import 'recent_items.dart';
import 'starred_items.dart';
import 'storage_usage_controller.dart';

final List<ProviderOrFamily> listViews = [
  recentItemsProvider,
  starredItemsProvider,
  sharedWithMeProvider,
  storageUsageControllerProvider,
  trashControllerProvider,
];

void refreshFileViews(void Function(ProviderOrFamily provider) invalidate) {
  for (final provider in <ProviderOrFamily>[folderControllerProvider, childrenControllerProvider, ...listViews]) {
    invalidate(provider);
  }
}
