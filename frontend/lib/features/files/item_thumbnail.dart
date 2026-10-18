import 'dart:async';
import 'dart:typed_data';

import 'package:dio/dio.dart';
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../api/generated/models/item_entry.dart';
import '../../api/generated/models/preview_size.dart';
import 'files_service.dart';

class ItemThumbnail extends ConsumerStatefulWidget {
  const ItemThumbnail({super.key, required this.entry, required this.placeholder, this.fit = BoxFit.cover});

  static const maximumAttempts = 15;
  static const retryDelay = Duration(seconds: 2);

  final ItemEntry entry;
  final Widget placeholder;
  final BoxFit fit;

  @override
  ConsumerState<ItemThumbnail> createState() => _ItemThumbnailState();
}

class _ItemThumbnailState extends ConsumerState<ItemThumbnail> {
  CancelToken _cancel = CancelToken();
  Uint8List? _bytes;

  @override
  void initState() {
    super.initState();
    _start();
  }

  @override
  void didUpdateWidget(ItemThumbnail oldWidget) {
    super.didUpdateWidget(oldWidget);
    final entry = widget.entry;
    final previous = oldWidget.entry;
    if (previous.id != entry.id ||
        previous.hasPreview != entry.hasPreview ||
        previous.previewId != entry.previewId) {
      _cancel.cancel();
      _cancel = CancelToken();
      _bytes = null;
      _start();
    }
  }

  @override
  void dispose() {
    _cancel.cancel();
    super.dispose();
  }

  void _start() {
    if (widget.entry.hasPreview) {
      unawaited(_load(widget.entry.id, widget.entry.previewId, _cancel));
    }
  }

  Future<void> _load(String id, String? previewId, CancelToken cancel) async {
    final files = ref.read(filesServiceProvider);
    try {
      for (var attempt = 0; attempt < ItemThumbnail.maximumAttempts && !cancel.isCancelled; attempt++) {
        final bytes = await files.preview(id, PreviewSize.thumbnail, previewId, cancel);
        if (bytes != null) {
          if (mounted && !cancel.isCancelled) {
            setState(() => _bytes = bytes);
          }
          return;
        }
        await Future<void>.delayed(ItemThumbnail.retryDelay);
      }
    } on DioException {
      return;
    }
  }

  @override
  Widget build(BuildContext context) {
    final bytes = _bytes;
    if (bytes == null) {
      return widget.placeholder;
    }
    return Image.memory(bytes, fit: widget.fit, gaplessPlayback: true, errorBuilder: (_, _, _) => widget.placeholder);
  }
}
