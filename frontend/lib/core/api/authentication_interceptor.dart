import 'package:dio/dio.dart';

class AuthenticationInterceptor extends Interceptor {
  AuthenticationInterceptor(this._onUnauthorized);

  final void Function() _onUnauthorized;

  @override
  void onError(DioException err, ErrorInterceptorHandler handler) {
    if (err.response?.statusCode == 401) {
      _onUnauthorized();
    }
    handler.next(err);
  }
}
