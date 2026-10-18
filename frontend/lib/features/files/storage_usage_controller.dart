import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../api/generated/models/storage_usage.dart';
import '../../core/providers/retry.dart';
import 'files_service.dart';

final storageUsageControllerProvider = AsyncNotifierProvider.autoDispose<StorageUsageController, StorageUsage>(
  StorageUsageController.new,
  retry: noRetry,
);

class StorageUsageController extends AsyncNotifier<StorageUsage> {
  @override
  Future<StorageUsage> build() => ref.read(filesServiceProvider).usage();
}
