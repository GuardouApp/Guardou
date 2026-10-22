import 'dart:async';
import 'dart:typed_data';

import 'package:dio/dio.dart';
import 'package:flutter/material.dart';

import '../../api/generated/models/item_entry.dart';
import '../../api/generated/models/preview_size.dart';
import 'file_source.dart';
import 'preview_loader.dart';

class ItemThumbnail extends StatefulWidget {
  const ItemThumbnail({
    super.key,
    required this.entry,
    required this.source,
    required this.placeholder,
    this.fit = BoxFit.cover,
  });

  final ItemEntry entry;
  final FileSource source;
  final Widget placeholder;
  final BoxFit fit;

  @override
  State<ItemThumbnail> createState() => _ItemThumbnailState();
}

class _ItemThumbnailState extends State<ItemThumbnail> {
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
    if (previous.id != entry.id || previous.hasPreview != entry.hasPreview) {
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
      unawaited(_load(widget.entry.id, _cancel));
    }
  }

  Future<void> _load(String id, CancelToken cancel) async {
    final bytes = await PreviewLoader.load(widget.source, id, PreviewSize.thumbnail, cancel);
    if (bytes != null && mounted && !cancel.isCancelled) {
      setState(() => _bytes = bytes);
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
