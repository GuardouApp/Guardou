import 'package:dio/dio.dart';

import 'api_problem.dart';

class ProblemDetailsInterceptor extends Interceptor {
  @override
  void onError(DioException err, ErrorInterceptorHandler handler) {
    handler.next(err.copyWith(error: ApiProblem.fromResponse(err.response)));
  }
}
