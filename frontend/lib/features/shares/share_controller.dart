import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../core/providers/retry.dart';
import 'share_access.dart';
import 'shares_service.dart';

final shareControllerProvider = AsyncNotifierProvider.autoDispose.family<ShareController, String?, String>(
  ShareController.new,
  retry: noRetry,
);

class ShareController extends AsyncNotifier<String?> {
  ShareController(this.itemId);

  final String itemId;

  SharesService get _shares => ref.read(sharesServiceProvider);

  @override
  Future<String?> build() => _shares.link(itemId);

  Future<void> change(ShareAccess access) async {
    state = const AsyncLoading();
    state = await AsyncValue.guard(
      () => switch (access) {
        ShareAccess.anyoneWithLink => _shares.share(itemId),
        ShareAccess.restricted => _shares.stopSharing(itemId).then((_) => null),
      },
    );
  }
}
