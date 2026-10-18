import 'package:dio/dio.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../api/generated/api_client.dart';
import '../../features/auth/session_controller.dart';
import 'antiforgery_interceptor.dart';
import 'authentication_interceptor.dart';
import 'problem_details_interceptor.dart';

final apiBaseUrlProvider = Provider<String>((ref) => Uri.base.origin);

final antiforgeryInterceptorProvider = Provider<AntiforgeryInterceptor>((ref) {
  final tokenClient = ApiClient(Dio(BaseOptions(baseUrl: ref.watch(apiBaseUrlProvider))));
  return AntiforgeryInterceptor(() async => (await tokenClient.auth.getAntiforgeryToken()).token);
});

final dioProvider = Provider<Dio>((ref) {
  final dio = Dio(BaseOptions(baseUrl: ref.watch(apiBaseUrlProvider)));
  dio.interceptors.addAll([
    ref.watch(antiforgeryInterceptorProvider),
    AuthenticationInterceptor(() => ref.read(sessionControllerProvider.notifier).expire()),
    ProblemDetailsInterceptor(),
  ]);
  ref.onDispose(dio.close);
  return dio;
});

final apiClientProvider = Provider<ApiClient>((ref) => ApiClient(ref.watch(dioProvider)));
