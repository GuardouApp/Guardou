import 'dart:async';

import 'package:dio/dio.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../core/api/api_problem.dart';
import '../configuration/configuration_controller.dart';
import 'session_controller.dart';

enum AuthFailure {
  invalidCredentials,
  passwordLoginDisabled,
  lockedOut,
  rateLimited,
  rejected,
  registrationClosed,
  signUpClosed,
  wrongPassword,
  oidcFailed,
  oidcNoAccount,
  oidcRejected,
  unknown;

  static AuthFailure ofLogin(ApiProblem problem) => switch (problem) {
    ApiProblem(isRateLimited: true) => rateLimited,
    ApiProblem(isLockedOut: true) => lockedOut,
    ApiProblem(status: 401) => invalidCredentials,
    ApiProblem(status: 403) => passwordLoginDisabled,
    _ => unknown,
  };

  static AuthFailure ofRegistration(ApiProblem problem) => _ofCreation(problem, registrationClosed);

  static AuthFailure ofSignUp(ApiProblem problem) => _ofCreation(problem, signUpClosed);

  static AuthFailure ofPasswordChange(ApiProblem problem) => switch (problem) {
    ApiProblem(isRateLimited: true) => rateLimited,
    ApiProblem(:final codes) when codes.contains('PasswordMismatch') => wrongPassword,
    ApiProblem(status: 400) => rejected,
    _ => unknown,
  };

  static AuthFailure ofOidc(String code) => switch (code) {
    'noAccount' => oidcNoAccount,
    'rejected' => oidcRejected,
    _ => oidcFailed,
  };

  static AuthFailure _ofCreation(ApiProblem problem, AuthFailure closed) => switch (problem) {
    ApiProblem(isRateLimited: true) => rateLimited,
    ApiProblem(status: 400) => rejected,
    ApiProblem(status: 403) => closed,
    _ => unknown,
  };
}

final authFormControllerProvider = NotifierProvider.autoDispose<AuthFormController, AsyncValue<void>>(
  AuthFormController.new,
);

class AuthFormController extends Notifier<AsyncValue<void>> {
  @override
  AsyncValue<void> build() => const AsyncData(null);

  SessionController get _session => ref.read(sessionControllerProvider.notifier);

  Future<void> login({required String username, required String password, required bool rememberMe}) =>
      _run(AuthFailure.ofLogin, () => _session.login(username: username, password: password, rememberMe: rememberMe));

  Future<void> register({
    required String username,
    required String displayName,
    required String email,
    required String password,
  }) => _run(
    AuthFailure.ofRegistration,
    () => _refreshingConfiguration(
      () => _session.register(username: username, displayName: displayName, email: email, password: password),
    ),
  );

  Future<void> signUp({
    required String username,
    required String displayName,
    required String email,
    required String password,
  }) => _run(
    AuthFailure.ofSignUp,
    () => _refreshingConfiguration(
      () => _session.signUp(username: username, displayName: displayName, email: email, password: password),
    ),
  );

  Future<bool> changePassword({required String currentPassword, required String newPassword}) => _run(
    AuthFailure.ofPasswordChange,
    () => _session.changePassword(currentPassword: currentPassword, newPassword: newPassword),
  );

  Future<void> _refreshingConfiguration(Future<void> Function() action) async {
    try {
      await action();
    } finally {
      unawaited(ref.read(configurationControllerProvider.notifier).refresh());
    }
  }

  Future<bool> _run(AuthFailure Function(ApiProblem problem) failureOf, Future<void> Function() action) async {
    state = const AsyncLoading();
    try {
      await action();
      state = const AsyncData(null);
      return true;
    } on DioException catch (error, stackTrace) {
      final problem = error.error;
      state = AsyncError(problem is ApiProblem ? failureOf(problem) : AuthFailure.unknown, stackTrace);
      return false;
    }
  }
}
