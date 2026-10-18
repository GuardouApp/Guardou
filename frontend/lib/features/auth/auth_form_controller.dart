import 'dart:async';

import 'package:dio/dio.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../core/api/api_problem.dart';
import '../configuration/configuration_controller.dart';
import 'session_controller.dart';

enum AuthFailure {
  invalidCredentials,
  rejected,
  registrationClosed,
  unknown;

  static AuthFailure fromStatus(int status) => switch (status) {
    400 => rejected,
    401 => invalidCredentials,
    403 => registrationClosed,
    _ => unknown,
  };
}

final authFormControllerProvider = NotifierProvider.autoDispose<AuthFormController, AsyncValue<void>>(
  AuthFormController.new,
);

class AuthFormController extends Notifier<AsyncValue<void>> {
  @override
  AsyncValue<void> build() => const AsyncData(null);

  Future<void> login({required String username, required String password, required bool rememberMe}) => _run(
    () => ref
        .read(sessionControllerProvider.notifier)
        .login(username: username, password: password, rememberMe: rememberMe),
  );

  Future<void> register({required String username, required String email, required String password}) => _run(() async {
    try {
      await ref.read(sessionControllerProvider.notifier).register(username: username, email: email, password: password);
    } finally {
      unawaited(ref.read(configurationControllerProvider.notifier).refresh());
    }
  });

  Future<void> _run(Future<void> Function() action) async {
    state = const AsyncLoading();
    try {
      await action();
      state = const AsyncData(null);
    } on DioException catch (error, stackTrace) {
      final problem = error.error;
      state = AsyncError(
        problem is ApiProblem ? AuthFailure.fromStatus(problem.status) : AuthFailure.unknown,
        stackTrace,
      );
    }
  }
}
