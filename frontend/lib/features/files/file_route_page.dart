import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../api/generated/models/item_details.dart';
import '../../app/router/routes.dart';
import '../../core/providers/retry.dart';
import '../../l10n/generated/app_localizations.dart';
import 'files_service.dart';
import 'folder_page.dart';
import 'folder_states.dart';

final _fileProvider = FutureProvider.autoDispose.family<ItemDetails, String>(
  (ref, id) => ref.read(filesServiceProvider).item(id),
  retry: noRetry,
);

class FileRoutePage extends ConsumerWidget {
  const FileRoutePage({super.key, required this.fileId});

  final String fileId;

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    return switch (ref.watch(_fileProvider(fileId))) {
      AsyncValue(hasValue: true, :final value?) => FolderPage(
        key: ValueKey(value.entry.parentId),
        folderId: value.entry.parentId ?? Routes.rootFolderId,
        focusedItemId: fileId,
      ),
      AsyncValue(:final error?) => FolderErrorView(
        isRoot: false,
        error: error,
        onRetry: () => ref.invalidate(_fileProvider(fileId)),
      ),
      _ => Center(child: CircularProgressIndicator(semanticsLabel: AppLocalizations.of(context).loading)),
    };
  }
}
