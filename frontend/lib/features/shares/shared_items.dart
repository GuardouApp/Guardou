import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../api/generated/models/item_details.dart';
import '../../api/generated/models/item_entry.dart';
import '../../core/providers/retry.dart';
import 'shares_service.dart';

typedef SharedItemKey = ({String token, String id});

final sharedRootProvider = FutureProvider.autoDispose.family<ItemEntry, String>(
  (ref, token) => ref.read(sharesServiceProvider).sharedItem(token),
  retry: noRetry,
);

final sharedDetailsProvider = FutureProvider.autoDispose.family<ItemDetails, SharedItemKey>(
  (ref, key) => ref.read(sharesServiceProvider).sharedDetails(key.token, key.id),
  retry: noRetry,
);
