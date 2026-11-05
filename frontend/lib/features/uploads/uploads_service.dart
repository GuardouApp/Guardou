import 'dart:typed_data';

import 'package:dio/dio.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../api/generated/api_client.dart';
import '../../api/generated/models/complete_upload_request.dart';
import '../../api/generated/models/conflict_policy.dart';
import '../../api/generated/models/item_entry.dart';
import '../../api/generated/models/start_upload_request.dart';
import '../../core/api/api_providers.dart';
import '../../core/browser/browser_files.dart';

final uploadsServiceProvider = Provider<UploadsService>(
  (ref) => UploadsService(ref.watch(apiClientProvider), ref.watch(dioProvider)),
);

class UploadsService {
  const UploadsService(this._api, this._dio);

  static const _tusHeaders = {'Tus-Resumable': '1.0.0'};
  static const _offsetHeader = 'Upload-Offset';
  static const _offsetContentType = 'application/offset+octet-stream';

  final ApiClient _api;
  final Dio _dio;

  Future<String> start(String folderId, LocalFile file, ConflictPolicy conflict) async {
    final session = await _api.uploads.startUpload(
      body: StartUploadRequest(
        folderId: folderId,
        fileName: file.name,
        relativePath: file.relativePath,
        size: file.size,
        modified: file.lastModified?.toUtc(),
        conflict: conflict,
      ),
    );
    return session.id;
  }

  Future<int> offset(String id) async {
    final response = await _dio.head<void>(_contentPath(id), options: Options(headers: _tusHeaders));
    return int.parse(response.headers.value(_offsetHeader) ?? '0');
  }

  Future<void> append(
    String id,
    int offset,
    Uint8List chunk, {
    required CancelToken cancelToken,
    required ProgressCallback onProgress,
  }) => _dio.patch<void>(
    _contentPath(id),
    data: chunk,
    cancelToken: cancelToken,
    onSendProgress: onProgress,
    options: Options(contentType: _offsetContentType, headers: {..._tusHeaders, _offsetHeader: offset}),
  );

  Future<ItemEntry> complete(String id, ConflictPolicy conflict) => _api.uploads.completeUpload(
    id: id,
    body: CompleteUploadRequest(conflict: conflict),
  );

  Future<void> cancel(String id) => _api.uploads.cancelUpload(id: id);

  String _contentPath(String id) => '/api/uploads/content/${Uri.encodeComponent(id)}';
}
