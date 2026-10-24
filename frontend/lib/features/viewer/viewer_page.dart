import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../api/generated/models/item_entry.dart';
import '../../app/router/routes.dart';
import '../../app/theme/app_theme.dart';
import '../../l10n/generated/app_localizations.dart';
import '../files/files_service.dart';
import '../files/folder_controller.dart';
import '../files/folder_states.dart';
import 'media_siblings.dart';
import 'media_viewer.dart';

class ViewerPage extends ConsumerWidget {
  const ViewerPage({super.key, required this.fileId});

  final String fileId;

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final l10n = AppLocalizations.of(context);

    return Theme(
      data: AppTheme.dark(),
      child: switch (ref.watch(fileDetailsProvider(fileId))) {
        AsyncValue(hasValue: true, :final value?) => _viewer(ref, value.entry),
        AsyncValue(:final error?) => Scaffold(
          appBar: AppBar(leading: const ViewerCloseButton(location: Routes.files)),
          body: FolderErrorView(
            isRoot: false,
            error: error,
            onRetry: () => ref.invalidate(fileDetailsProvider(fileId)),
          ),
        ),
        _ => Scaffold(
          body: Center(child: CircularProgressIndicator(semanticsLabel: l10n.loading)),
        ),
      },
    );
  }

  Widget _viewer(WidgetRef ref, ItemEntry entry) {
    final parentId = entry.parentId;
    final close = parentId == null ? Routes.shared : Routes.fileItem(entry.id);
    return MediaViewer(
      entry: entry,
      source: ref.watch(filesServiceProvider),
      siblings: parentId == null ? const [] : ref.watch(mediaSiblingsProvider(parentId)),
      viewerRoute: Routes.viewer,
      leading: ViewerCloseButton(location: close),
      closeLocation: close,
    );
  }
}
