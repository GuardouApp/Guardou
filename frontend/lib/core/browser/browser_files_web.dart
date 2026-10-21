import 'dart:async';
import 'dart:js_interop';

import 'package:flutter/painting.dart';
import 'package:web/web.dart' as web;

import 'browser_files.dart';

BrowserFiles createBrowserFiles() => _WebBrowserFiles();

class _WebBrowserFiles implements BrowserFiles {
  static const _filesType = 'Files';
  static const _copyEffect = 'copy';
  static const _newTab = '_blank';
  static const _imageTypes = 'image/*';

  @override
  late final Stream<FileDragEvent> dragEvents = _listenToDrags().asBroadcastStream();

  @override
  Future<List<LocalFile>> pickFiles() => _pick(directory: false);

  @override
  Future<List<LocalFile>> pickFolder() => _pick(directory: true);

  @override
  Future<LocalFile?> pickImage() async =>
      (await _pick(directory: false, multiple: false, accept: _imageTypes)).firstOrNull;

  @override
  void download(Uri uri) {
    final anchor = web.HTMLAnchorElement()
      ..href = uri.toString()
      ..download = '';
    web.document.body?.append(anchor);
    anchor
      ..click()
      ..remove();
  }

  @override
  void open(Uri uri) => web.window.open(uri.toString(), _newTab);

  Future<List<LocalFile>> _pick({required bool directory, bool multiple = true, String accept = ''}) {
    final completer = Completer<List<LocalFile>>();
    final input = web.HTMLInputElement()
      ..type = 'file'
      ..multiple = multiple
      ..accept = accept
      ..webkitdirectory = directory;
    input.style.display = 'none';
    web.document.body?.append(input);
    input
      ..onchange = ((web.Event _) => _complete(completer, input, _filesOf(input.files, relative: directory))).toJS
      ..oncancel = ((web.Event _) => _complete(completer, input, const [])).toJS
      ..click();
    return completer.future;
  }

  void _complete(Completer<List<LocalFile>> completer, web.HTMLInputElement input, List<LocalFile> files) {
    input.remove();
    if (!completer.isCompleted) {
      completer.complete(files);
    }
  }

  Stream<FileDragEvent> _listenToDrags() {
    final controller = StreamController<FileDragEvent>();
    final subscriptions = [
      web.EventStreamProviders.dragOverEvent.forTarget(web.document).listen((event) {
        if (_carriesFiles(event)) {
          event.preventDefault();
          (event as web.DragEvent).dataTransfer?.dropEffect = _copyEffect;
          controller.add(FileDragOver(_positionOf(event)));
        }
      }),
      web.EventStreamProviders.dragLeaveEvent.forTarget(web.document).listen((event) {
        if (event.relatedTarget == null) {
          controller.add(const FileDragLeave());
        }
      }),
      web.EventStreamProviders.dropEvent.forTarget(web.document).listen((event) {
        if (_carriesFiles(event)) {
          event.preventDefault();
          final transfer = (event as web.DragEvent).dataTransfer;
          controller.add(
            FileDrop(_positionOf(event), transfer == null ? Future.value(const []) : _droppedFiles(transfer)),
          );
        }
      }),
    ];
    controller.onCancel = () => Future.wait(subscriptions.map((subscription) => subscription.cancel()));
    return controller.stream;
  }

  bool _carriesFiles(web.MouseEvent event) =>
      (event as web.DragEvent).dataTransfer?.types.toDart.any((type) => type.toDart == _filesType) ?? false;

  Offset _positionOf(web.MouseEvent event) => Offset(event.clientX.toDouble(), event.clientY.toDouble());

  List<LocalFile> _filesOf(web.FileList? files, {required bool relative}) => [
    for (final file in [for (var index = 0; index < (files?.length ?? 0); index++) ?files?.item(index)])
      _localFile(file, relative ? file.webkitRelativePath : null),
  ];

  LocalFile _localFile(web.File file, String? relativePath) => LocalFile(
    name: file.name,
    size: file.size,
    relativePath: relativePath,
    lastModified: DateTime.fromMillisecondsSinceEpoch(file.lastModified, isUtc: true),
    read: () async => (await file.arrayBuffer().toDart).toDart.asUint8List(),
  );

  Future<List<LocalFile>> _droppedFiles(web.DataTransfer transfer) async {
    final entries = <web.FileSystemEntry>[
      for (var index = 0; index < transfer.items.length; index++) ?transfer.items[index].webkitGetAsEntry(),
    ];
    final files = await Future.wait(entries.map((entry) => _readEntry(entry, '')));
    return files.expand((list) => list).toList();
  }

  Future<List<LocalFile>> _readEntry(web.FileSystemEntry entry, String parent) async {
    final path = parent.isEmpty ? entry.name : '$parent/${entry.name}';
    if (entry.isFile) {
      return [_localFile(await _fileOf(entry as web.FileSystemFileEntry), path)];
    }
    final children = await _childrenOf((entry as web.FileSystemDirectoryEntry).createReader());
    final files = await Future.wait(children.map((child) => _readEntry(child, path)));
    return files.expand((list) => list).toList();
  }

  Future<web.File> _fileOf(web.FileSystemFileEntry entry) {
    final completer = Completer<web.File>();
    entry.file(
      ((web.File file) => completer.complete(file)).toJS,
      ((web.DOMException error) => completer.completeError(error)).toJS,
    );
    return completer.future;
  }

  Future<List<web.FileSystemEntry>> _childrenOf(web.FileSystemDirectoryReader reader) async {
    final children = <web.FileSystemEntry>[];
    while (true) {
      final batch = await _readBatch(reader);
      if (batch.isEmpty) {
        return children;
      }
      children.addAll(batch);
    }
  }

  Future<List<web.FileSystemEntry>> _readBatch(web.FileSystemDirectoryReader reader) {
    final completer = Completer<List<web.FileSystemEntry>>();
    reader.readEntries(
      ((JSArray<web.FileSystemEntry> entries) => completer.complete(entries.toDart)).toJS,
      ((web.DOMException error) => completer.completeError(error)).toJS,
    );
    return completer.future;
  }
}
