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

  static const quotaExceededCode = 'quotaExceeded';
  static const copyPendingCode = 'copyPending';
  static const lockedOutCode = 'lockedOut';
  static const rateLimitedCode = 'rateLimited';
  static const passwordChangeRequiredCode = 'passwordChangeRequired';

  bool get isUnauthorized => status == 401;

  bool get isQuotaExceeded => codes.contains(quotaExceededCode);

  bool get isCopyPending => codes.contains(copyPendingCode);

  bool get isLockedOut => codes.contains(lockedOutCode);

  bool get isRateLimited => status == 429;

  bool get isPasswordChangeRequired => codes.contains(passwordChangeRequiredCode);

  @override
  String toString() => 'ApiProblem($status, $title)';
}
