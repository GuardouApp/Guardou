import 'package:flutter/foundation.dart';
import 'package:flutter/painting.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import 'browser_files_stub.dart' if (dart.library.js_interop) 'browser_files_web.dart';

@immutable
class LocalFile {
  const LocalFile({
    required this.name,
    required this.size,
    required this.read,
    this.relativePath,
    this.lastModified,
  });

  final String name;
  final int size;
  final String? relativePath;
  final DateTime? lastModified;
  final Future<Uint8List> Function() read;
}

sealed class FileDragEvent {
  const FileDragEvent();
}

class FileDragOver extends FileDragEvent {
  const FileDragOver(this.position);

  final Offset position;
}

class FileDragLeave extends FileDragEvent {
  const FileDragLeave();
}

class FileDrop extends FileDragEvent {
  const FileDrop(this.position, this.files);

  final Offset position;
  final Future<List<LocalFile>> files;
}

abstract interface class BrowserFiles {
  Future<List<LocalFile>> pickFiles();

  Future<List<LocalFile>> pickFolder();

  void download(Uri uri);

  void open(Uri uri);

  Stream<FileDragEvent> get dragEvents;
}

final browserFilesProvider = Provider<BrowserFiles>((ref) => createBrowserFiles());
