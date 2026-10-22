import 'package:dio/dio.dart';
import 'package:flutter/foundation.dart';

import '../../api/generated/models/preview_size.dart';

abstract class FileSource {
  const FileSource(this.dio, this.baseUri);

  static const _previewPending = 202;

  @protected
  final Dio dio;

  @protected
  final Uri baseUri;

  @protected
  List<String> itemPath(String id);

  Uri downloadUri(String id) => _itemUri(id, 'download');

  Uri viewUri(String id) => _itemUri(id, 'view');

  Uri? creatorPictureUri(String id) => null;

  Future<Uint8List?> preview(String id, PreviewSize size, CancelToken cancelToken) async {
    final response = await dio.get<List<int>>(
      _itemUri(id, 'preview').path,
      queryParameters: {'size': size.json},
      options: Options(responseType: ResponseType.bytes),
      cancelToken: cancelToken,
    );
    final data = response.data;
    return response.statusCode == _previewPending || data == null ? null : Uint8List.fromList(data);
  }

  Future<Uint8List> original(String id, CancelToken cancelToken) async {
    final response = await dio.get<List<int>>(
      viewUri(id).path,
      options: Options(responseType: ResponseType.bytes),
      cancelToken: cancelToken,
    );
    return Uint8List.fromList(response.data ?? const []);
  }

  Uri _itemUri(String id, String action) => baseUri.replace(pathSegments: [...itemPath(id), action]);
}
