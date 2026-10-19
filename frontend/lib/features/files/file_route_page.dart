import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../app/router/routes.dart';
import '../../l10n/generated/app_localizations.dart';
import 'folder_controller.dart';
import 'folder_page.dart';
import 'folder_states.dart';

class FileRoutePage extends ConsumerWidget {
  const FileRoutePage({super.key, required this.fileId});

  final String fileId;

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    return switch (ref.watch(fileDetailsProvider(fileId))) {
      AsyncValue(hasValue: true, :final value?) => FolderPage(
        key: ValueKey(value.entry.parentId),
        folderId: value.entry.parentId ?? Routes.rootFolderId,
        focusedItemId: fileId,
      ),
      AsyncValue(:final error?) => FolderErrorView(
        isRoot: false,
        error: error,
        onRetry: () => ref.invalidate(fileDetailsProvider(fileId)),
      ),
      _ => Center(child: CircularProgressIndicator(semanticsLabel: AppLocalizations.of(context).loading)),
    };
  }
}
