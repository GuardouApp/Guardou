import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../core/realtime/realtime_event.dart';

final itemChangesProvider = NotifierProvider<ItemChangesController, ItemsChanged?>(ItemChangesController.new);

class ItemChangesController extends Notifier<ItemsChanged?> {
  @override
  ItemsChanged? build() => null;

  void publish(ItemsChanged change) => state = change;
}

void refreshOnItemChanges(Ref ref, Iterable<String?> Function() folderIds) {
  ref.listen(itemChangesProvider, (_, change) {
    if (change != null && change.touchesAny(folderIds())) {
      ref.invalidateSelf();
    }
  });
}
