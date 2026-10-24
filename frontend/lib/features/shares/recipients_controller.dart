import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../api/generated/models/user_summary.dart';
import '../../core/providers/retry.dart';
import 'shares_service.dart';

final recipientsControllerProvider = AsyncNotifierProvider.autoDispose
    .family<RecipientsController, List<UserSummary>, String>(RecipientsController.new, retry: noRetry);

class RecipientsController extends AsyncNotifier<List<UserSummary>> {
  RecipientsController(this.itemId);

  final String itemId;

  SharesService get _shares => ref.read(sharesServiceProvider);

  @override
  Future<List<UserSummary>> build() => _shares.recipients(itemId);

  Future<void> add(UserSummary user) async {
    state = await AsyncValue.guard(() => _shares.addRecipient(itemId, user.id));
  }

  Future<void> remove(UserSummary user) async {
    final remaining = [...?state.value]..removeWhere((recipient) => recipient.id == user.id);
    state = await AsyncValue.guard(() => _shares.removeRecipient(itemId, user.id).then((_) => remaining));
  }
}
