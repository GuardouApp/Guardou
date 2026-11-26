import 'package:dio/dio.dart';

import 'api_problem.dart';

class AuthenticationInterceptor extends Interceptor {
  AuthenticationInterceptor({required this.onUnauthorized, required this.onPasswordChangeRequired});

  final void Function() onUnauthorized;
  final void Function() onPasswordChangeRequired;

  @override
  void onError(DioException err, ErrorInterceptorHandler handler) {
    final problem = ApiProblem.fromResponse(err.response);
    if (problem.isUnauthorized) {
      onUnauthorized();
    } else if (problem.isPasswordChangeRequired) {
      onPasswordChangeRequired();
    }
    handler.next(err);
  }
}
