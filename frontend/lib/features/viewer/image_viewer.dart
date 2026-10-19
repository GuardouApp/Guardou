import 'dart:async';
import 'dart:typed_data';

import 'package:dio/dio.dart';
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:lucide_icons_flutter/lucide_icons.dart';

import '../../api/generated/models/item_entry.dart';
import '../../api/generated/models/preview_size.dart';
import '../../app/theme/app_tokens.dart';
import '../../l10n/generated/app_localizations.dart';
import '../files/files_service.dart';
import '../files/preview_loader.dart';
import 'unsupported_viewer.dart';

class ImageViewer extends ConsumerStatefulWidget {
  const ImageViewer({super.key, required this.entry});

  static const _browserImageTypes = {'image/jpeg', 'image/png', 'image/gif', 'image/webp', 'image/bmp', 'image/avif'};

  final ItemEntry entry;

  @override
  ConsumerState<ImageViewer> createState() => _ImageViewerState();
}

class _ImageViewerState extends ConsumerState<ImageViewer> {
  static const _maximumScale = 8.0;
  static const _quarterTurnsInCircle = 4;

  final _cancel = CancelToken();
  Uint8List? _preview;
  Uint8List? _original;
  bool _loading = true;
  int _quarterTurns = 0;

  ItemEntry get _entry => widget.entry;

  @override
  void initState() {
    super.initState();
    unawaited(_load());
  }

  @override
  void dispose() {
    _cancel.cancel();
    super.dispose();
  }

  Future<void> _load() async {
    final files = ref.read(filesServiceProvider);
    await Future.wait([
      if (_entry.hasPreview) _loadPreview(files),
      if (ImageViewer._browserImageTypes.contains(_entry.mimeType)) _loadOriginal(files),
    ]);
    if (mounted) {
      setState(() => _loading = false);
    }
  }

  Future<void> _loadPreview(FilesService files) async {
    final bytes = await PreviewLoader.load(files, _entry.id, PreviewSize.large, _entry.previewId, _cancel);
    if (bytes != null && mounted && _original == null) {
      setState(() => _preview = bytes);
    }
  }

  Future<void> _loadOriginal(FilesService files) async {
    try {
      final bytes = await files.original(_entry.id, _cancel);
      if (mounted) {
        setState(() => _original = bytes);
      }
    } on DioException {
      return;
    }
  }

  void _rotate() => setState(() => _quarterTurns = (_quarterTurns + 1) % _quarterTurnsInCircle);

  Widget _image(Uint8List? bytes, Widget fallback) => bytes == null
      ? fallback
      : Image.memory(bytes, fit: BoxFit.contain, gaplessPlayback: true, errorBuilder: (_, _, _) => fallback);

  @override
  Widget build(BuildContext context) {
    final l10n = AppLocalizations.of(context);
    final spacing = AppTokens.of(context).spacing;
    final unsupported = UnsupportedViewer(entry: _entry);
    if (_original == null && _preview == null) {
      return _loading ? Center(child: CircularProgressIndicator(semanticsLabel: l10n.loading)) : unsupported;
    }

    return Stack(
      children: [
        Positioned.fill(
          child: InteractiveViewer(
            maxScale: _maximumScale,
            child: Center(
              child: RotatedBox(quarterTurns: _quarterTurns, child: _image(_original, _image(_preview, unsupported))),
            ),
          ),
        ),
        Positioned(
          right: spacing,
          bottom: spacing,
          child: FloatingActionButton.small(
            heroTag: null,
            tooltip: l10n.viewerRotate,
            onPressed: _rotate,
            child: const Icon(LucideIcons.rotateCw),
          ),
        ),
      ],
    );
  }
}
