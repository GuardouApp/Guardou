import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../api/generated/models/user_account.dart';
import '../../core/providers/retry.dart';
import 'users_service.dart';

final usersControllerProvider = AsyncNotifierProvider.autoDispose<UsersController, List<UserAccount>>(
  UsersController.new,
  retry: noRetry,
);

class UsersController extends AsyncNotifier<List<UserAccount>> {
  @override
  Future<List<UserAccount>> build() => ref.read(usersServiceProvider).list();
}
