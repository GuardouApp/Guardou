import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../api/generated/models/item_entry.dart';
import '../../app/theme/app_tokens.dart';
import '../../core/browser/browser_embeds.dart';
import '../files/file_presentation.dart';
import '../files/files_service.dart';
import 'image_viewer.dart';
import 'unsupported_viewer.dart';
import 'viewer_kind.dart';

class ViewerContent extends ConsumerWidget {
  const ViewerContent({super.key, required this.entry});

  static const _audioWidth = 480.0;
  static const _audioHeight = 56.0;
  static const _audioIconSize = 96.0;

  final ItemEntry entry;

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final embeds = ref.watch(browserEmbedsProvider);
    final source = ref.watch(filesServiceProvider).viewUri(entry.id);

    return switch (ViewerKind.of(entry)) {
      ViewerKind.image => ImageViewer(entry: entry),
      ViewerKind.video => embeds.video(source),
      ViewerKind.audio => _audio(context, embeds.audio(source)),
      ViewerKind.pdf => embeds.document(source),
      ViewerKind.unsupported => UnsupportedViewer(entry: entry),
    };
  }

  Widget _audio(BuildContext context, Widget player) {
    final spacing = AppTokens.of(context).spacing;
    return Center(
      child: Padding(
        padding: EdgeInsets.all(spacing),
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            Icon(iconFor(entry), size: _audioIconSize),
            SizedBox(height: spacing * 2),
            ConstrainedBox(
              constraints: const BoxConstraints(maxWidth: _audioWidth),
              child: SizedBox(height: _audioHeight, child: player),
            ),
          ],
        ),
      ),
    );
  }
}
