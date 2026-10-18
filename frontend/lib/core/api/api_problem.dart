import 'package:dio/dio.dart';

class ApiProblem implements Exception {
  const ApiProblem({required this.status, this.title, this.detail, this.codes = const []});

  factory ApiProblem.fromResponse(Response<Object?>? response) {
    final data = response?.data;
    final body = data is Map<String, Object?> ? data : const <String, Object?>{};
    return ApiProblem(
      status: response?.statusCode ?? 0,
      title: body['title'] as String?,
      detail: body['detail'] as String?,
      codes: [...?(body['codes'] as List<Object?>?)?.whereType<String>()],
    );
  }

  final int status;
  final String? title;
  final String? detail;
  final List<String> codes;

  bool get isUnauthorized => status == 401;

  @override
  String toString() => 'ApiProblem($status, $title)';
}
