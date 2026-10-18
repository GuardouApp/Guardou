import 'package:dio/dio.dart';

class AntiforgeryInterceptor extends QueuedInterceptor {
  AntiforgeryInterceptor(this._loadToken);

  static const headerName = 'X-XSRF-TOKEN';
  static const _safeMethods = {'GET', 'HEAD', 'OPTIONS', 'TRACE'};

  final Future<String> Function() _loadToken;
  String? _token;

  void reset() => _token = null;

  @override
  Future<void> onRequest(RequestOptions options, RequestInterceptorHandler handler) async {
    if (_safeMethods.contains(options.method.toUpperCase())) {
      handler.next(options);
      return;
    }
    try {
      final token = _token ??= await _loadToken();
      options.headers[headerName] = token;
      handler.next(options);
    } on DioException catch (error) {
      handler.reject(error);
    }
  }
}
