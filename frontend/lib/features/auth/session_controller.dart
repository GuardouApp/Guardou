import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../api/generated/models/current_user.dart';
import 'auth_service.dart';

final sessionControllerProvider = AsyncNotifierProvider<SessionController, CurrentUser?>(SessionController.new);

class SessionController extends AsyncNotifier<CurrentUser?> {
  @override
  Future<CurrentUser?> build() => ref.read(authServiceProvider).restore();

  Future<void> login({required String username, required String password, required bool rememberMe}) async {
    final user = await ref
        .read(authServiceProvider)
        .login(username: username, password: password, rememberMe: rememberMe);
    state = AsyncData(user);
  }

  Future<void> register({required String username, required String email, required String password}) async {
    state = AsyncData(
      await ref.read(authServiceProvider).register(username: username, email: email, password: password),
    );
  }

  Future<void> refresh() async {
    state = AsyncData(await ref.read(authServiceProvider).restore());
  }

  Future<void> logout() async {
    await ref.read(authServiceProvider).logout();
    state = const AsyncData(null);
  }

  void expire() {
    if (state.value != null) {
      state = const AsyncData(null);
    }
  }
}
