import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../api/generated/models/item_entry.dart';
import '../../core/providers/retry.dart';
import 'shares_service.dart';

final sharedWithMeProvider = FutureProvider.autoDispose<List<ItemEntry>>(
  (ref) => ref.read(sharesServiceProvider).sharedWithMe(),
  retry: noRetry,
);
