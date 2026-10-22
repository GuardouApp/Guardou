import 'dart:typed_data';

import 'package:dio/dio.dart';

import '../../api/generated/models/preview_size.dart';
import 'file_source.dart';

abstract final class PreviewLoader {
  static const maximumAttempts = 15;
  static const retryDelay = Duration(seconds: 2);

  static Future<Uint8List?> load(FileSource files, String id, PreviewSize size, CancelToken cancel) async {
    try {
      for (var attempt = 0; attempt < maximumAttempts && !cancel.isCancelled; attempt++) {
        final bytes = await files.preview(id, size, cancel);
        if (bytes != null) {
          return bytes;
        }
        await Future<void>.delayed(retryDelay);
      }
    } on DioException {
      return null;
    }
    return null;
  }
}
