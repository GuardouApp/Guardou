import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../api/generated/models/item_entry.dart';
import '../../core/providers/retry.dart';
import 'files_service.dart';

final starredItemsProvider = FutureProvider.autoDispose<List<ItemEntry>>(
  (ref) => ref.read(filesServiceProvider).starred(),
  retry: noRetry,
);
