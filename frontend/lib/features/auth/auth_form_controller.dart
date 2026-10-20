import 'dart:async';

import 'package:dio/dio.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../core/api/api_problem.dart';
import '../configuration/configuration_controller.dart';
import 'session_controller.dart';

enum AuthFailure {
  invalidCredentials,
  passwordLoginDisabled,
  rejected,
  registrationClosed,
  oidcFailed,
  oidcNoAccount,
  oidcRejected,
  unknown;

  static AuthFailure ofLogin(int status) => switch (status) {
    401 => invalidCredentials,
    403 => passwordLoginDisabled,
    _ => unknown,
  };

  static AuthFailure ofRegistration(int status) => switch (status) {
    400 => rejected,
    403 => registrationClosed,
    _ => unknown,
  };

  static AuthFailure ofOidc(String code) => switch (code) {
    'noAccount' => oidcNoAccount,
    'rejected' => oidcRejected,
    _ => oidcFailed,
  };
}

final authFormControllerProvider = NotifierProvider.autoDispose<AuthFormController, AsyncValue<void>>(
  AuthFormController.new,
);

class AuthFormController extends Notifier<AsyncValue<void>> {
  @override
  AsyncValue<void> build() => const AsyncData(null);

  Future<void> login({required String username, required String password, required bool rememberMe}) => _run(
    AuthFailure.ofLogin,
    () => ref
        .read(sessionControllerProvider.notifier)
        .login(username: username, password: password, rememberMe: rememberMe),
  );

  Future<void> register({required String username, required String email, required String password}) =>
      _run(AuthFailure.ofRegistration, () async {
        try {
          await ref
              .read(sessionControllerProvider.notifier)
              .register(username: username, email: email, password: password);
        } finally {
          unawaited(ref.read(configurationControllerProvider.notifier).refresh());
        }
      });

  Future<void> _run(AuthFailure Function(int status) failureOf, Future<void> Function() action) async {
    state = const AsyncLoading();
    try {
      await action();
      state = const AsyncData(null);
    } on DioException catch (error, stackTrace) {
      final problem = error.error;
      state = AsyncError(problem is ApiProblem ? failureOf(problem.status) : AuthFailure.unknown, stackTrace);
    }
  }
}
