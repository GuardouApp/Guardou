import 'package:dio/dio.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../api/generated/models/current_user.dart';
import '../../api/generated/models/login_request.dart';
import '../../api/generated/models/register_request.dart';
import '../../api/generated/api_client.dart';
import '../../core/api/antiforgery_interceptor.dart';
import '../../core/api/api_providers.dart';

final authServiceProvider = Provider<AuthService>(
  (ref) => AuthService(ref.watch(apiClientProvider), ref.watch(antiforgeryInterceptorProvider)),
);

class AuthService {
  const AuthService(this._api, this._antiforgery);

  static const _oidcLoginPath = '/api/auth/oidc/login';
  static const _returnUrlParameter = 'returnUrl';

  final ApiClient _api;
  final AntiforgeryInterceptor _antiforgery;

  Future<CurrentUser?> restore() async {
    try {
      return await _api.auth.getCurrentUser();
    } on DioException catch (error) {
      if (error.response?.statusCode == 401) {
        return null;
      }
      rethrow;
    }
  }

  Future<CurrentUser> login({required String username, required String password, required bool rememberMe}) async {
    await _api.auth.login(
      body: LoginRequest(username: username, password: password, rememberMe: rememberMe),
    );
    _antiforgery.reset();
    return _api.auth.getCurrentUser();
  }

  Future<CurrentUser> register({
    required String username,
    required String displayName,
    required String email,
    required String password,
  }) async {
    await _api.auth.register(
      body: RegisterRequest(username: username, displayName: displayName, email: email, password: password),
    );
    _antiforgery.reset();
    return _api.auth.getCurrentUser();
  }

  Uri oidcLogin({String? returnUrl}) =>
      Uri(path: _oidcLoginPath, queryParameters: returnUrl == null ? null : {_returnUrlParameter: returnUrl});

  Future<Uri?> logout() async {
    final result = await _api.auth.logout();
    _antiforgery.reset();
    return switch (result.redirectUrl) {
      final url? => Uri.tryParse(url),
      null => null,
    };
  }
}
