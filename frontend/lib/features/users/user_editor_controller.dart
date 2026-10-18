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

  Future<UserAccount?> save(String? id, UserRequest request) async {
    state = const AsyncLoading();
    try {
      final result = await (id == null ? _users.create(request) : _users.update(id, request));
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
