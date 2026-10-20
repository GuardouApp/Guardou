import 'package:dio/dio.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../api/generated/models/user_account.dart';
import '../../api/generated/models/user_request.dart';
import 'user_failure.dart';
import 'users_controller.dart';
import 'users_service.dart';

final userEditorControllerProvider = NotifierProvider.autoDispose<UserEditorController, AsyncValue<void>>(
  UserEditorController.new,
);

class UserEditorController extends Notifier<AsyncValue<void>> {
  @override
  AsyncValue<void> build() => const AsyncData(null);

  UsersService get _users => ref.read(usersServiceProvider);

  Future<UserAccount?> save(String? id, UserRequest request) =>
      _run(() => id == null ? _users.create(request) : _users.update(id, request));

  Future<UserAccount?> removeOidcLogin(String id) => _run(() => _users.removeOidcLogin(id));

  Future<UserAccount?> _run(Future<UserAccount> Function() action) async {
    state = const AsyncLoading();
    try {
      final result = await action();
      state = const AsyncData(null);
      return result;
    } on DioException catch (error, stackTrace) {
      state = AsyncError(UserFailure.of(error), stackTrace);
      return null;
    } finally {
      ref.invalidate(usersControllerProvider);
    }
  }
}
