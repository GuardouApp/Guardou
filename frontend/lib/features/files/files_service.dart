import 'dart:typed_data';

import 'package:dio/dio.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../api/generated/api_client.dart';
import '../../api/generated/models/conflict_policy.dart';
import '../../api/generated/models/item_details.dart';
import '../../api/generated/models/item_entry.dart';
import '../../api/generated/models/item_selection.dart';
import '../../api/generated/models/name_request.dart';
import '../../api/generated/models/preview_size.dart';
import '../../api/generated/models/storage_usage.dart';
import '../../api/generated/models/transfer_request.dart';
import '../../core/api/api_providers.dart';
import '../../core/browser/browser_files.dart';

final filesServiceProvider = Provider<FilesService>(
  (ref) => FilesService(ref.watch(apiClientProvider), ref.watch(dioProvider), Uri.parse(ref.watch(apiBaseUrlProvider))),
);

class FilesService {
  const FilesService(this._api, this._dio, this._baseUri);

  static const _previewPending = 202;

  final ApiClient _api;
  final Dio _dio;
  final Uri _baseUri;

  Future<ItemDetails> item(String id) => _api.items.getItem(id: id);

  Future<List<ItemEntry>> children(String id) => _api.items.getItemChildren(id: id);

  Future<StorageUsage> usage() => _api.items.getStorageUsage();

  Future<ItemEntry> createFolder(String parentId, String name) => _api.items.createFolder(
    id: parentId,
    body: NameRequest(name: name),
  );

  Future<ItemEntry> rename(String id, String name) => _api.items.renameItem(
    id: id,
    body: NameRequest(name: name),
  );

  Future<ItemEntry> move(String id, String destinationId, ConflictPolicy conflict) => _api.items.moveItem(
    id: id,
    body: TransferRequest(destinationId: destinationId, conflict: conflict),
  );

  Future<ItemEntry> copy(String id, String destinationId, ConflictPolicy conflict) => _api.items.copyItem(
    id: id,
    body: TransferRequest(destinationId: destinationId, conflict: conflict),
  );

  Future<void> trash(List<String> ids) => _api.items.moveItemsToTrash(body: ItemSelection(ids: ids));

  Future<ItemEntry> upload(
    String folderId,
    LocalFile file, {
    required ConflictPolicy conflict,
    ProgressCallback? onProgress,
  }) async {
    final form = FormData.fromMap({
      'file': MultipartFile.fromBytes(await file.read(), filename: file.name),
      'conflict': conflict.json,
      'relativePath': ?file.relativePath,
      'modified': ?file.lastModified?.toUtc().toIso8601String(),
    });
    final response = await _dio.post<Map<String, Object?>>(
      '/api/items/${Uri.encodeComponent(folderId)}/files',
      data: form,
      onSendProgress: onProgress,
    );
    return ItemEntry.fromJson(response.data ?? const {});
  }

  Future<Uint8List?> preview(String id, PreviewSize size, String? previewId, CancelToken cancelToken) async {
    final item = Uri.encodeComponent(id);
    final response = await _dio.get<List<int>>(
      previewId == null ? '/api/items/$item/preview' : '/api/items/$item/previews/${Uri.encodeComponent(previewId)}',
      queryParameters: {'size': size.json},
      options: Options(responseType: ResponseType.bytes),
      cancelToken: cancelToken,
    );
    final data = response.data;
    return response.statusCode == _previewPending || data == null ? null : Uint8List.fromList(data);
  }

  Future<Uint8List> original(String id, CancelToken cancelToken) async {
    final response = await _dio.get<List<int>>(
      viewUri(id).path,
      options: Options(responseType: ResponseType.bytes),
      cancelToken: cancelToken,
    );
    return Uint8List.fromList(response.data ?? const []);
  }

  Uri downloadUri(String id) => _itemUri(id, 'download');

  Uri viewUri(String id) => _itemUri(id, 'view');

  Uri archiveUri(List<String> ids) => _baseUri.replace(path: '/api/items/archive', queryParameters: {'ids': ids});

  Uri _itemUri(String id, String action) => _baseUri.replace(pathSegments: ['api', 'items', id, action]);
}
