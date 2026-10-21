import 'package:dio/dio.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../api/generated/api_client.dart';
import '../../core/api/api_providers.dart';
import '../../core/browser/browser_files.dart';

final profilePictureServiceProvider = Provider<ProfilePictureService>(
  (ref) => ProfilePictureService(ref.watch(apiClientProvider), ref.watch(dioProvider)),
);

class ProfilePictureService {
  const ProfilePictureService(this._api, this._dio);

  final ApiClient _api;
  final Dio _dio;

  Future<void> replace(String userId, LocalFile file) async {
    final form = FormData.fromMap({'file': MultipartFile.fromBytes(await file.read(), filename: file.name)});
    await _dio.put<void>('/api/users/${Uri.encodeComponent(userId)}/picture', data: form);
  }

  Future<void> delete(String userId) => _api.profilePictures.deleteUserPicture(id: userId);
}
