import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../api/generated/models/item_entry.dart';
import '../../core/providers/retry.dart';
import 'files_service.dart';

final recentItemsProvider = FutureProvider.autoDispose<List<ItemEntry>>(
  (ref) => ref.read(filesServiceProvider).recent(),
  retry: noRetry,
);
